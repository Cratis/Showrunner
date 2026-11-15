// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageManager.Productions;
using Cratis.Showrunner.StageManager.Understudies.Catalog;
using Cratis.Showrunner.StageManager.Understudies.Jobs;
using Cratis.Showrunner.StageManager.Understudies.Keys;
using Cratis.Showrunner.StageManager.Understudies.Reporting;
using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies;

/// <summary>
/// Brings an Understudy into the cluster, or says why it cannot be: checks the spec and the inputs it names, enforces the bucket rules, schedules the backup CronJobs,
/// reads the catalog for the status, and reports the outcome of the jobs that finished. Nothing is scheduled until every input the Understudy names is in hand.
/// </summary>
/// <param name="cluster">The cluster.</param>
/// <param name="storages">The factory of object storage.</param>
/// <param name="reporter">The reporter.</param>
/// <param name="options">The options of the operator.</param>
/// <param name="time">The time provider.</param>
/// <param name="logger">The logger.</param>
public class UnderstudyReconciler(
    IUnderstudyCluster cluster,
    IObjectStorageFactory storages,
    UnderstudyReporter reporter,
    UnderstudyOptions options,
    TimeProvider time,
    ILogger<UnderstudyReconciler> logger)
{
    const int ReportedJobsKept = 50;
    static readonly TimeSpan _matchSlack = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Reconciles the Understudy and writes its status.
    /// </summary>
    /// <param name="understudy">The Understudy.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The phase the Understudy ended in.</returns>
    public async Task<UnderstudyPhase> ReconcileAsync(Understudy understudy, CancellationToken cancellationToken)
    {
        understudy.Status ??= new();
        var status = new UnderstudyStatusBuilder(understudy, time);
        await RunAsync(understudy, status, cancellationToken);
        status.Write();
        await cluster.UpdateStatusAsync(understudy, cancellationToken);
        logger.UnderstudyPhase(understudy.Metadata.NamespaceProperty, understudy.Metadata.Name, status.Phase);
        return status.Phase;
    }

    static IReadOnlyList<LastBackupStatus> LastBackupsOf(IEnumerable<CatalogEntry> entries)
    {
        var latestPerScope = entries
            .GroupBy(entry => (entry.Manifest.EventStore, entry.Manifest.Namespace))
            .Select(group => group.OrderByDescending(entry => entry.Manifest.TakenAt).First())
            .ToList();

        return
        [
            .. latestPerScope
                .SelectMany(entry => entry.Manifest.Entries.Select(item => (entry.Manifest.TakenAt, Item: item)))
                .GroupBy(pair => pair.Item.Type)
                .OrderBy(group => group.Key)
                .Select(group => new LastBackupStatus
                {
                    Type = group.Key,
                    TakenAt = group.Max(pair => pair.TakenAt),
                    SizeBytes = group.Sum(pair => pair.Item.SizeBytes),
                    Files = group.Count()
                })
        ];
    }

    static UnderstudyPhase PhaseOf(bool degraded, int lastBackups)
    {
        if (degraded)
        {
            return UnderstudyPhase.Degraded;
        }

        return lastBackups > 0 ? UnderstudyPhase.Healthy : UnderstudyPhase.Scheduled;
    }

    static bool Finished(V1Job job) => job.Status?.Succeeded > 0 || job.Status?.Failed > 0;

    static DateTimeOffset FinishedAt(V1Job job) =>
        new(DateTime.SpecifyKind(job.Status?.CompletionTime ?? job.Status?.Conditions?.Select(condition => condition.LastTransitionTime).FirstOrDefault(time => time is not null) ?? DateTime.UnixEpoch, DateTimeKind.Utc), TimeSpan.Zero);

    static string KindOf(V1Job job) => job.Metadata.Labels is { } labels && labels.TryGetValue(UnderstudyMetadata.KindLabel, out var kind) ? kind : UnderstudyJobKinds.Backup;

    async Task RunAsync(Understudy understudy, UnderstudyStatusBuilder status, CancellationToken cancellationToken)
    {
        var problems = UnderstudyValidator.Validate(understudy.Spec);
        if (problems.Count > 0)
        {
            status.Set(UnderstudyConditions.SpecValid, false, problems[0].Reason, string.Join(' ', problems.Select(problem => problem.Message)));
            status.End(UnderstudyPhase.Blocked);
            return;
        }

        status.Set(UnderstudyConditions.SpecValid, true, "Valid", "The spec is valid.");
        var @namespace = understudy.Metadata.NamespaceProperty;
        var production = await cluster.GetProductionAsync(@namespace, understudy.Spec.ProductionRef, cancellationToken);
        if (production is null)
        {
            status.Set(UnderstudyConditions.ProductionFound, false, "ProductionNotFound", $"The Production '{understudy.Spec.ProductionRef}' does not exist in this namespace.");
            status.End(UnderstudyPhase.Blocked);
            return;
        }

        if (string.IsNullOrEmpty(production.Status?.ConnectionSecretName))
        {
            status.Set(UnderstudyConditions.ProductionFound, false, "ConnectionNotAvailable", $"The Production '{production.Metadata.Name}' has not written its connection Secret yet.");
            status.End(UnderstudyPhase.Blocked);
            return;
        }

        status.Set(UnderstudyConditions.ProductionFound, true, "Found", $"Backing up the Production '{production.Metadata.Name}'.");

        var data = await SettingsAsync(@namespace, understudy.Spec.Storage, cancellationToken);
        if (data.Missing is not null)
        {
            status.Set(UnderstudyConditions.StorageReady, false, "SecretNotFound", data.Missing);
            status.End(UnderstudyPhase.Blocked);
            return;
        }

        var catalogs = new List<CatalogEntry>();
        var keyCatalogs = new List<CatalogEntry>();
        var (storageOk, unclaimed) = await InspectDataAsync(understudy, data.Settings!, status, catalogs, cancellationToken);

        var keysReady = await PrepareKeysAsync(understudy, production, status, keyCatalogs, cancellationToken);

        await cluster.ApplyAsync(UnderstudyWorkload.BackupCronJob(understudy, production, options), cancellationToken);
        if (keysReady)
        {
            await cluster.ApplyAsync(UnderstudyWorkload.KeysCronJob(understudy, production, options), cancellationToken);
            await RequestVaultSnapshotAsync(understudy, production, cancellationToken);
        }

        if (understudy.Spec.Suspend)
        {
            status.Set(UnderstudyConditions.Scheduled, true, "Suspended", "The backups are paused.");
        }
        else
        {
            status.Set(UnderstudyConditions.Scheduled, true, "Scheduled", $"Databases are backed up on '{understudy.Spec.Schedule}'.");
        }

        var outcome = await ReportJobsAsync(understudy, production, catalogs, keyCatalogs, cancellationToken);
        var last = LastBackupsOf(catalogs).Concat(LastBackupsOf(keyCatalogs)).ToList();
        understudy.Status.LastBackups = last;
        understudy.Status.Unclaimed = [.. unclaimed];

        var healthy = !outcome.DataFailed && !outcome.KeysFailed;
        if (!healthy)
        {
            status.Set(UnderstudyConditions.BackupsHealthy, false, "LatestRunFailed", "The latest backup run failed. See the Kubernetes events of this Understudy and the log of its job.");
        }
        else if (last.Count > 0)
        {
            status.Set(UnderstudyConditions.BackupsHealthy, true, "Healthy", "The latest backup run succeeded.");
        }
        else
        {
            status.Set(UnderstudyConditions.BackupsHealthy, false, "NoBackupYet", "No backup has been taken yet.");
        }

        var keysRefused = understudy.Spec.Keys is not null && UnderstudyWorkload.HasVault(production) && !keysReady;
        var degraded = !healthy || !storageOk || keysRefused;
        reporter.Observe(production.Metadata.Name, last, !degraded, unclaimed.Count);
        status.End(PhaseOf(degraded, last.Count));
    }

    async Task<(ObjectStorageSettings? Settings, string? Missing)> SettingsAsync(string @namespace, UnderstudyStorage spec, CancellationToken cancellationToken)
    {
        async Task<string?> ValueAsync(string key) => await cluster.GetSecretValueAsync(@namespace, spec.CredentialsSecretName, key, cancellationToken);

        switch (spec.Provider)
        {
            case ObjectStorageProviders.S3:
                var accessKey = await ValueAsync(UnderstudyStorage.AccessKeyKey);
                var secretKey = await ValueAsync(UnderstudyStorage.SecretKeyKey);
                return accessKey is null || secretKey is null
                    ? (null, $"The Secret '{spec.CredentialsSecretName}' must hold the keys '{UnderstudyStorage.AccessKeyKey}' and '{UnderstudyStorage.SecretKeyKey}'.")
                    : (Settings(spec) with { AccessKey = accessKey, SecretKey = secretKey }, null);

            case ObjectStorageProviders.AzureBlob:
                var account = await ValueAsync(UnderstudyStorage.AccountNameKey);
                var key = await ValueAsync(UnderstudyStorage.AccountKeyKey);
                return account is null || key is null
                    ? (null, $"The Secret '{spec.CredentialsSecretName}' must hold the keys '{UnderstudyStorage.AccountNameKey}' and '{UnderstudyStorage.AccountKeyKey}'.")
                    : (Settings(spec) with { AccountName = account, AccountKey = key }, null);

            default:
                return (Settings(spec), null);
        }
    }

    ObjectStorageSettings Settings(UnderstudyStorage spec) => new()
    {
        Provider = spec.Provider,
        Endpoint = spec.Endpoint,
        Region = spec.Region,
        Bucket = spec.Bucket,
        PathStyle = spec.PathStyle,
        BlobServiceUrl = spec.BlobServiceUrl,
        VolumePath = UnderstudyJobEnvironment.VolumePath
    };

    async Task<(bool Reachable, IReadOnlyList<string> Unclaimed)> InspectDataAsync(
        Understudy understudy,
        ObjectStorageSettings settings,
        UnderstudyStatusBuilder status,
        List<CatalogEntry> catalog,
        CancellationToken cancellationToken)
    {
        if (settings.Provider == ObjectStorageProviders.Volume)
        {
            // The operator does not mount the volume, so it can read neither the bucket rules nor the catalog.
            status.Set(UnderstudyConditions.StorageReady, true, "Volume", "Backups are written to a volume. The operator does not read it, so the catalog is not in this status.");
            status.Set(UnderstudyConditions.LifecycleEnforced, false, "NotSupported", "A volume has no expiry rule. Pruning keeps the maximum age while the backup job runs.");
            return (true, []);
        }

        var storage = storages.Create(settings);
        try
        {
            var days = understudy.Spec.MaxAgeDays;
            var applied = await storage.TryEnforceExpiryAsync(days, cancellationToken);
            var state = await storage.InspectAsync(cancellationToken);
            status.Set(UnderstudyConditions.StorageReady, true, "Reachable", "The bucket can be reached.");
            if (state.ExpiryDays == days)
            {
                status.Set(UnderstudyConditions.LifecycleEnforced, true, "Enforced", $"The bucket expires every object after {days} days.");
            }
            else
            {
                status.Set(UnderstudyConditions.LifecycleEnforced, false, applied ? "NotReadBack" : "NotSupported", $"The {storage.Provider} provider does not show an expiry rule of {days} days. Retention then depends on the backup job running.");
            }

            var production = understudy.Spec.ProductionRef;
            catalog.AddRange(await new BackupCatalog(storage).ListAsync(production, understudy.Spec.ToRetentionPolicy(), time.GetUtcNow(), cancellationToken));
            await using var plan = await storage.OpenReadAsync(PlanReport.KeyOf(production), cancellationToken);
            return (true, plan is null ? [] : PlanReport.FromJson(plan).Unclaimed);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            status.Set(UnderstudyConditions.StorageReady, false, "Unreachable", $"The bucket could not be read ({exception.GetType().Name}).");
            return (false, []);
        }
        finally
        {
            (storage as IDisposable)?.Dispose();
        }
    }

    async Task<bool> PrepareKeysAsync(Understudy understudy, Production production, UnderstudyStatusBuilder status, List<CatalogEntry> catalog, CancellationToken cancellationToken)
    {
        if (understudy.Spec.Keys is not { } keys)
        {
            return false;
        }

        if (!UnderstudyWorkload.HasVault(production))
        {
            status.Set(UnderstudyConditions.KeyBackupsReady, false, "NoVault", "The Production has no Vault, so its keys travel inside the Events backups and nothing separate is taken.");
            return false;
        }

        if (string.IsNullOrEmpty(production.Spec.Vault.Address))
        {
            status.Set(UnderstudyConditions.KeyBackupsReady, false, "VaultAddressMissing", "The Production names no Vault address.");
            return false;
        }

        var @namespace = understudy.Metadata.NamespaceProperty;
        var certificate = await cluster.GetSecretValueAsync(@namespace, keys.CertificateSecret.Name, keys.CertificateSecret.Key ?? UnderstudyKeys.DefaultCertificateKey, cancellationToken);
        var token = await cluster.GetSecretValueAsync(@namespace, keys.VaultTokenSecret.Name, keys.VaultTokenSecret.Key ?? UnderstudyKeys.DefaultTokenKey, cancellationToken);
        if (certificate is null || token is null)
        {
            status.Set(UnderstudyConditions.KeyBackupsReady, false, "SecretNotFound", "The Secrets of the certificate and the Vault token must exist and hold their keys.");
            return false;
        }

        if (certificate.Contains("PRIVATE KEY", StringComparison.Ordinal))
        {
            status.Set(UnderstudyConditions.KeyBackupsReady, false, "CertificateHasPrivateKey", "The certificate Secret holds a private key. Keys are encrypted to the public certificate only; keep the private key out of the cluster.");
            return false;
        }

        try
        {
            using var parsed = KeyEncryptor.CertificateFromPem(certificate);
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            status.Set(UnderstudyConditions.KeyBackupsReady, false, "CertificateInvalid", "The certificate Secret does not hold a certificate in PEM form.");
            return false;
        }

        var settings = await SettingsAsync(@namespace, keys.Storage, cancellationToken);
        if (settings.Missing is not null)
        {
            status.Set(UnderstudyConditions.KeyBackupsReady, false, "SecretNotFound", settings.Missing);
            return false;
        }

        var storage = storages.Create(settings.Settings!);
        try
        {
            var check = await KeyBucketGuard.EnsureAsync(storage, keys.MaxAgeDays, cancellationToken);
            status.Set(UnderstudyConditions.KeyBackupsReady, check.Holds, check.Reason, check.Message);
            if (check.Holds)
            {
                catalog.AddRange(await new BackupCatalog(storage).ListAsync(understudy.Spec.ProductionRef, understudy.Spec.ToRetentionPolicy() with { MaxAgeDays = keys.MaxAgeDays }, time.GetUtcNow(), cancellationToken));
            }

            return check.Holds;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            status.Set(UnderstudyConditions.KeyBackupsReady, false, "Unreachable", $"The keys bucket could not be read ({exception.GetType().Name}).");
            return false;
        }
        finally
        {
            (storage as IDisposable)?.Dispose();
        }
    }

    async Task RequestVaultSnapshotAsync(Understudy understudy, Production production, CancellationToken cancellationToken)
    {
        var annotations = understudy.Metadata.Annotations;
        if (annotations is null || !annotations.TryGetValue(UnderstudyMetadata.VaultSnapshotAnnotation, out var request) || string.IsNullOrEmpty(request))
        {
            return;
        }

        if (string.Equals(understudy.Status.LastVaultSnapshotRequest, request, StringComparison.Ordinal))
        {
            return;
        }

        await cluster.TryCreateAsync(UnderstudyWorkload.VaultSnapshotJob(understudy, production, options, request), cancellationToken);
        understudy.Status.LastVaultSnapshotRequest = request;
    }

    async Task<JobOutcome> ReportJobsAsync(Understudy understudy, Production production, List<CatalogEntry> data, List<CatalogEntry> keys, CancellationToken cancellationToken)
    {
        var jobs = (await cluster.ListJobsAsync(understudy.Metadata.NamespaceProperty, understudy.Metadata.Name, cancellationToken))
            .Where(Finished)
            .OrderBy(FinishedAt)
            .ToList();

        var reported = understudy.Status.ReportedJobs.ToList();
        foreach (var job in jobs.Where(job => !reported.Contains(job.Metadata.Name)))
        {
            var kind = KindOf(job);
            if (job.Status.Succeeded > 0)
            {
                var started = new DateTimeOffset(DateTime.SpecifyKind(job.Status.StartTime ?? DateTime.UnixEpoch, DateTimeKind.Utc), TimeSpan.Zero);
                var finished = FinishedAt(job);
                var source = kind == UnderstudyJobKinds.Backup ? data : keys;
                foreach (var entry in source.Where(entry => entry.Manifest.TakenAt >= started - _matchSlack && entry.Manifest.TakenAt <= finished + _matchSlack))
                {
                    await reporter.BackupTakenAsync(understudy, entry, kind, cancellationToken);
                }
            }
            else
            {
                var condition = job.Status.Conditions?.FirstOrDefault(item => item.Type == "Failed");
                var reason = $"The {kind} job failed ({condition?.Reason ?? "Failed"}). See the log of its pod.";
                await reporter.BackupFailedAsync(understudy, production.Metadata.Name, job.Metadata.Name, kind, FinishedAt(job), reason, cancellationToken);
            }

            reported.Add(job.Metadata.Name);
        }

        understudy.Status.ReportedJobs = [.. reported.TakeLast(ReportedJobsKept)];
        return new(
            jobs.LastOrDefault(job => KindOf(job) == UnderstudyJobKinds.Backup) is { } lastData && lastData.Status.Failed > 0,
            jobs.LastOrDefault(job => KindOf(job) == UnderstudyJobKinds.Keys) is { } lastKeys && lastKeys.Status.Failed > 0);
    }

    sealed record JobOutcome(bool DataFailed, bool KeysFailed);
}
