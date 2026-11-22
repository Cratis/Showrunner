// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageManager.Productions;
using Cratis.Showrunner.StageManager.Understudies.Jobs;
using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies;

/// <summary>
/// Builds the CronJobs and Jobs an <see cref="Understudy"/> schedules. Nothing secret is copied into them: credentials, connection strings, the Vault token
/// and the certificate are read from their Secrets by the pod itself.
/// </summary>
public static class UnderstudyWorkload
{
    const string WorkVolume = "work";
    const string BackupsVolume = "backups";
    const string WorkPath = "/work";

    /// <summary>
    /// Builds the CronJob that backs up the databases and prunes.
    /// </summary>
    /// <param name="understudy">The Understudy.</param>
    /// <param name="production">The Production it backs up.</param>
    /// <param name="options">The options of the operator.</param>
    /// <returns>The CronJob.</returns>
    public static V1CronJob BackupCronJob(Understudy understudy, Production production, UnderstudyOptions options) =>
        CronJob(understudy, UnderstudyNames.For(understudy).BackupCronJob, UnderstudyJobKinds.Backup, understudy.Spec.Schedule, JobSpec(understudy, production, options, UnderstudyJobKinds.Backup));

    /// <summary>
    /// Builds the CronJob that backs up the keys.
    /// </summary>
    /// <param name="understudy">The Understudy.</param>
    /// <param name="production">The Production it backs up.</param>
    /// <param name="options">The options of the operator.</param>
    /// <returns>The CronJob.</returns>
    public static V1CronJob KeysCronJob(Understudy understudy, Production production, UnderstudyOptions options) =>
        CronJob(understudy, UnderstudyNames.For(understudy).KeysCronJob, UnderstudyJobKinds.Keys, understudy.Spec.Keys!.Schedule, JobSpec(understudy, production, options, UnderstudyJobKinds.Keys));

    /// <summary>
    /// Builds the Job that takes a snapshot of the whole Vault.
    /// </summary>
    /// <param name="understudy">The Understudy.</param>
    /// <param name="production">The Production it backs up.</param>
    /// <param name="options">The options of the operator.</param>
    /// <param name="request">The value of the request annotation.</param>
    /// <returns>The Job.</returns>
    public static V1Job VaultSnapshotJob(Understudy understudy, Production production, UnderstudyOptions options, string request) => new()
    {
        ApiVersion = "batch/v1",
        Kind = "Job",
        Metadata = UnderstudyMetadata.Create(understudy, UnderstudyNames.For(understudy).VaultSnapshotJob(request), UnderstudyJobKinds.VaultSnapshot),
        Spec = JobSpec(understudy, production, options, UnderstudyJobKinds.VaultSnapshot)
    };

    /// <summary>
    /// Builds the configuration a job is told.
    /// </summary>
    /// <param name="understudy">The Understudy.</param>
    /// <param name="production">The Production.</param>
    /// <param name="kind">What the job does.</param>
    /// <returns>The configuration.</returns>
    public static UnderstudyJobConfiguration Configuration(Understudy understudy, Production production, string kind) => new(
        kind,
        production.Metadata.Name,
        production.Spec.Version,
        understudy.Spec.DatabasePrefix,
        [.. understudy.Spec.Databases],
        HasVault(production),
        understudy.Spec.ToRetentionPolicy(),
        understudy.Spec.Storage,
        understudy.Spec.Keys?.Storage,
        understudy.Spec.Keys?.MaxAgeDays ?? Catalog.RetentionPolicy.DefaultMaxAgeDays,
        production.Spec.Vault.Address,
        production.Spec.Database.SystemDatabaseName);

    /// <summary>
    /// Gets whether the Production keeps its keys in a Vault. Without one they travel inside the events.
    /// </summary>
    /// <param name="production">The Production.</param>
    /// <returns>True when it has a Vault.</returns>
    public static bool HasVault(Production production) => !string.Equals(production.Spec.Vault.Mode, "None", StringComparison.OrdinalIgnoreCase);

    static V1CronJob CronJob(Understudy understudy, string name, string kind, string schedule, V1JobSpec job) => new()
    {
        ApiVersion = "batch/v1",
        Kind = "CronJob",
        Metadata = UnderstudyMetadata.Create(understudy, name, kind),
        Spec = new()
        {
            Schedule = schedule,
            Suspend = understudy.Spec.Suspend,
            ConcurrencyPolicy = "Forbid",
            StartingDeadlineSeconds = 3600,
            SuccessfulJobsHistoryLimit = 3,
            FailedJobsHistoryLimit = 5,
            JobTemplate = new()
            {
                Metadata = new() { Labels = UnderstudyMetadata.Create(understudy, name, kind).Labels },
                Spec = job
            }
        }
    };

    static V1JobSpec JobSpec(Understudy understudy, Production production, UnderstudyOptions options, string kind)
    {
        var labels = UnderstudyMetadata.Create(understudy, understudy.Metadata.Name, kind).Labels;
        var volumes = new List<V1Volume> { new() { Name = WorkVolume, EmptyDir = new() { SizeLimit = new ResourceQuantity(understudy.Spec.WorkSize) } } };
        var mounts = new List<V1VolumeMount> { new() { Name = WorkVolume, MountPath = WorkPath } };
        var storage = kind == UnderstudyJobKinds.Backup ? understudy.Spec.Storage : understudy.Spec.Keys!.Storage;
        if (string.Equals(storage.Provider, ObjectStorageProviders.Volume, StringComparison.Ordinal))
        {
            volumes.Add(new() { Name = BackupsVolume, PersistentVolumeClaim = new() { ClaimName = storage.VolumeClaimName } });
            mounts.Add(new() { Name = BackupsVolume, MountPath = UnderstudyJobEnvironment.VolumePath });
        }

        return new()
        {
            BackoffLimit = 0,
            Template = new()
            {
                Metadata = new() { Labels = labels },
                Spec = new()
                {
                    RestartPolicy = "Never",
                    SecurityContext = new() { RunAsNonRoot = true, SeccompProfile = new() { Type = "RuntimeDefault" } },
                    Containers =
                    [
                        new()
                        {
                            Name = "understudy",
                            Image = string.IsNullOrEmpty(understudy.Spec.Image) ? options.DefaultImage : understudy.Spec.Image,
                            Args = [UnderstudyJobKinds.Command, kind],
                            Env = Environment(understudy, production, kind),
                            VolumeMounts = mounts,
                            SecurityContext = new()
                            {
                                AllowPrivilegeEscalation = false,
                                ReadOnlyRootFilesystem = true,
                                Capabilities = new() { Drop = ["ALL"] }
                            }
                        }
                    ],
                    Volumes = volumes
                }
            }
        };
    }

    static List<V1EnvVar> Environment(Understudy understudy, Production production, string kind)
    {
        var names = ProductionNames.For(production);
        var environment = new List<V1EnvVar>
        {
            new() { Name = UnderstudyJobEnvironment.Configuration, Value = Configuration(understudy, production, kind).ToJson() },
            new() { Name = UnderstudyJobEnvironment.WorkDirectory, Value = WorkPath },
            FromSecret(UnderstudyJobEnvironment.ChronicleConnection, names.Connection, ProductionSecretReference.DefaultKey)
        };

        if (kind == UnderstudyJobKinds.Backup)
        {
            var database = production.Spec.Database.IsExternal() ? production.Spec.Database.ConnectionSecretName : names.MongoDBConnection;
            environment.Add(FromSecret(UnderstudyJobEnvironment.MongoDBConnection, database, ProductionSecretReference.DefaultKey));
            environment.AddRange(Credentials(UnderstudyJobEnvironment.StoragePrefix, understudy.Spec.Storage));
            return environment;
        }

        var keys = understudy.Spec.Keys!;
        environment.AddRange(Credentials(UnderstudyJobEnvironment.KeysStoragePrefix, keys.Storage));
        environment.Add(FromSecret(UnderstudyJobEnvironment.VaultToken, keys.VaultTokenSecret.Name, keys.VaultTokenSecret.Key ?? UnderstudyKeys.DefaultTokenKey));
        environment.Add(FromSecret(UnderstudyJobEnvironment.KeysCertificate, keys.CertificateSecret.Name, keys.CertificateSecret.Key ?? UnderstudyKeys.DefaultCertificateKey));
        return environment;
    }

    static IEnumerable<V1EnvVar> Credentials(string prefix, UnderstudyStorage storage)
    {
        switch (storage.Provider)
        {
            case ObjectStorageProviders.S3:
                yield return FromSecret(prefix + UnderstudyJobEnvironment.AccessKey, storage.CredentialsSecretName, UnderstudyStorage.AccessKeyKey);
                yield return FromSecret(prefix + UnderstudyJobEnvironment.SecretKey, storage.CredentialsSecretName, UnderstudyStorage.SecretKeyKey);
                break;

            case ObjectStorageProviders.AzureBlob:
                yield return FromSecret(prefix + UnderstudyJobEnvironment.AccountName, storage.CredentialsSecretName, UnderstudyStorage.AccountNameKey);
                yield return FromSecret(prefix + UnderstudyJobEnvironment.AccountKey, storage.CredentialsSecretName, UnderstudyStorage.AccountKeyKey);
                break;
        }
    }

    static V1EnvVar FromSecret(string name, string secret, string key) => new()
    {
        Name = name,
        ValueFrom = new() { SecretKeyRef = new() { Name = secret, Key = key } }
    };
}
