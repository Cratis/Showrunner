// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Cratis.Showrunner.StageManager.Understudies.Catalog;
using Cratis.Showrunner.StageManager.Understudies.Discovery;
using Cratis.Showrunner.StageManager.Understudies.Keys;
using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies.Jobs;

/// <summary>
/// Backs up the keys of a Production, one run per namespace, each encrypted to the customer's public certificate before it leaves memory, into a bucket of its own.
/// It checks the bucket rules before any key is touched and refuses to start when they are not in place. Plaintext exists only in memory and is cleared once encrypted.
/// </summary>
/// <param name="storage">The storage of the keys bucket.</param>
/// <param name="directory">The Chronicle directory.</param>
/// <param name="vault">The Vault reader.</param>
/// <param name="certificate">The public certificate the keys are encrypted to.</param>
/// <param name="time">The time provider.</param>
/// <param name="logger">The logger.</param>
public class KeyBackupJob(
    IObjectStorage storage,
    IChronicleDirectory directory,
    IVaultKeyReader vault,
    X509Certificate2 certificate,
    TimeProvider time,
    ILogger<KeyBackupJob> logger)
{
    /// <summary>
    /// The name of the file of the keys of a namespace.
    /// </summary>
    public const string KeysFile = "Keys.cms";

    /// <summary>
    /// The name of the file of a Vault snapshot.
    /// </summary>
    public const string SnapshotFile = "VaultSnapshot.cms";

    /// <summary>
    /// Backs up the keys of every namespace.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>What the run did.</returns>
    public async Task<KeyBackupResult> RunAsync(UnderstudyJobConfiguration configuration, CancellationToken cancellationToken)
    {
        if (!configuration.HasVault)
        {
            // Without a Vault the keys travel inside the events, so there is nothing separate to take.
            return new([], [], null, new(0, 0, 0));
        }

        var check = await KeyBucketGuard.EnsureAsync(storage, configuration.KeysMaxAgeDays, cancellationToken);
        if (!check.Holds)
        {
            logger.KeyBackupsRefused(configuration.Production, check.Reason);
            return new([], [], check, new(0, 0, 0));
        }

        var takenAt = Truncate(time.GetUtcNow());
        var topology = await directory.GetTopologyAsync(cancellationToken);
        var taken = new List<BackupManifest>();
        var failed = new List<BackupFailure>();
        foreach (var eventStore in topology.EventStores)
        {
            foreach (var @namespace in eventStore.Namespaces)
            {
                try
                {
                    taken.Add(await BackupNamespaceAsync(configuration, new(eventStore.Name, @namespace), takenAt, cancellationToken));
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    // The message of an exception here could echo what Vault answered, so only its type is reported.
                    failed.Add(new(eventStore.Name, @namespace, $"The keys could not be backed up ({exception.GetType().Name})."));
                }
            }
        }

        var policy = configuration.Retention with { MaxAgeDays = configuration.KeysMaxAgeDays };
        var pruned = await new BackupPruner(storage, new BackupCatalog(storage)).PruneAsync(configuration.Production, policy, time.GetUtcNow(), cancellationToken);
        return new(taken, failed, check, pruned);
    }

    /// <summary>
    /// Takes a snapshot of the whole Vault, on demand.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>What the run did.</returns>
    public async Task<KeyBackupResult> SnapshotAsync(UnderstudyJobConfiguration configuration, CancellationToken cancellationToken)
    {
        var check = await KeyBucketGuard.EnsureAsync(storage, configuration.KeysMaxAgeDays, cancellationToken);
        if (!check.Holds)
        {
            logger.KeyBackupsRefused(configuration.Production, check.Reason);
            return new([], [], check, new(0, 0, 0));
        }

        var takenAt = Truncate(time.GetUtcNow());
        var runPrefix = BackupLayout.RunPrefix(configuration.Production, null, null, takenAt);
        var plaintext = await vault.TakeSnapshotAsync(cancellationToken);
        try
        {
            var manifest = await WriteAsync(configuration, null, null, takenAt, runPrefix, BackupType.VaultSnapshot, SnapshotFile, plaintext, cancellationToken);
            logger.VaultSnapshotTaken(configuration.Production);
            return new([manifest], [], check, new(0, 0, 0));
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plaintext);
        }
    }

    static DateTimeOffset Truncate(DateTimeOffset moment) => new(moment.Year, moment.Month, moment.Day, moment.Hour, moment.Minute, moment.Second, TimeSpan.Zero);

    async Task<BackupManifest> BackupNamespaceAsync(UnderstudyJobConfiguration configuration, PlannedKeys keys, DateTimeOffset takenAt, CancellationToken cancellationToken)
    {
        var secrets = await vault.ReadTreeAsync(keys.Path, cancellationToken);
        var plaintext = JsonSerializer.SerializeToUtf8Bytes(new
        {
            version = 1,
            eventStore = keys.EventStore,
            @namespace = keys.Namespace,
            takenAt,
            secrets = secrets.Select(secret => new { path = secret.Path, data = secret.Data })
        });

        try
        {
            var runPrefix = BackupLayout.RunPrefix(configuration.Production, keys.EventStore, keys.Namespace, takenAt);
            var manifest = await WriteAsync(configuration, keys.EventStore, keys.Namespace, takenAt, runPrefix, BackupType.Keys, KeysFile, plaintext, cancellationToken);
            logger.KeysBackedUp(configuration.Production, keys.EventStore, keys.Namespace);
            return manifest;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plaintext);
        }
    }

    async Task<BackupManifest> WriteAsync(
        UnderstudyJobConfiguration configuration,
        string? eventStore,
        string? @namespace,
        DateTimeOffset takenAt,
        string runPrefix,
        BackupType type,
        string file,
        byte[] plaintext,
        CancellationToken cancellationToken)
    {
        var encrypted = KeyEncryptor.Encrypt(plaintext, certificate);
        var checksum = Convert.ToHexStringLower(SHA256.HashData(encrypted));
        var key = runPrefix + file;
        try
        {
            await using (var content = new MemoryStream(encrypted))
            {
                await storage.PutAsync(key, content, cancellationToken);
            }

            var entry = new ManifestEntry(type, null, file, encrypted.Length, checksum);
            var manifest = new BackupManifest(BackupManifest.CurrentSchemaVersion, configuration.Production, eventStore, @namespace, takenAt, configuration.ChronicleVersion, configuration.Retention, [entry]);
            await using var json = new MemoryStream(manifest.ToJson());
            await storage.PutAsync(BackupLayout.ManifestKey(runPrefix), json, cancellationToken);
            return manifest;
        }
        catch
        {
            await storage.DeleteAsync(key, CancellationToken.None);
            throw;
        }
    }
}
