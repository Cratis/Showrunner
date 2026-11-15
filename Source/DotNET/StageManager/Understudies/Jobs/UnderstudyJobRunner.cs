// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageManager.Understudies.Discovery;
using Cratis.Showrunner.StageManager.Understudies.Keys;
using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies.Jobs;

/// <summary>
/// Runs a backup job from its environment: the entry point of the Understudy image. It exits with <see cref="Succeeded"/> only when every scope was backed up,
/// so a failed backup fails the Kubernetes Job and is reported by the operator; it never ends in success with partial output.
/// </summary>
public static class UnderstudyJobRunner
{
    /// <summary>
    /// The exit code of a run that backed up everything it set out to.
    /// </summary>
    public const int Succeeded = 0;

    /// <summary>
    /// The exit code of a run in which a scope failed or the job could not do its work.
    /// </summary>
    public const int Failed = 1;

    /// <summary>
    /// The exit code of a key backup run that was refused because the bucket rules are not in place. Nothing was read from Vault.
    /// </summary>
    public const int Refused = 2;

    /// <summary>
    /// Runs the job.
    /// </summary>
    /// <param name="kind">What to do, one of <see cref="UnderstudyJobKinds"/>.</param>
    /// <param name="environment">The environment variables.</param>
    /// <param name="loggers">The logger factory.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="storages">The factory of object storage. The supported providers when none is given.</param>
    /// <returns>The exit code.</returns>
    public static async Task<int> RunAsync(
        string kind,
        IReadOnlyDictionary<string, string> environment,
        ILoggerFactory loggers,
        CancellationToken cancellationToken,
        IObjectStorageFactory? storages = null)
    {
        var logger = loggers.CreateLogger(typeof(UnderstudyJobRunner).FullName!);
        try
        {
            var configuration = UnderstudyJobConfiguration.FromJson(Required(environment, UnderstudyJobEnvironment.Configuration));
            var work = environment.GetValueOrDefault(UnderstudyJobEnvironment.WorkDirectory) ?? Path.GetTempPath();
            var factory = storages ?? new ObjectStorageFactory();
            var directory = new ChronicleDirectory(Required(environment, UnderstudyJobEnvironment.ChronicleConnection));

            switch (kind)
            {
                case UnderstudyJobKinds.Backup:
                    return await BackupAsync(configuration, environment, factory, directory, work, loggers, cancellationToken);

                case UnderstudyJobKinds.Keys:
                case UnderstudyJobKinds.VaultSnapshot:
                    return await KeysAsync(kind, configuration, environment, factory, directory, loggers, cancellationToken);

                default:
                    logger.JobKindNotSupported(kind);
                    return Failed;
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // Only the type and the message of the exception are logged; neither holds a connection string or a key.
            logger.JobFailed(kind, exception.GetType().Name, exception.Message);
            return Failed;
        }
    }

    /// <summary>
    /// Builds the settings of a bucket from its spec and the credentials in the environment.
    /// </summary>
    /// <param name="spec">The spec of the storage.</param>
    /// <param name="prefix">The prefix of the variables that carry the credentials.</param>
    /// <param name="environment">The environment variables.</param>
    /// <returns>The settings.</returns>
    public static ObjectStorageSettings SettingsOf(UnderstudyStorage spec, string prefix, IReadOnlyDictionary<string, string> environment) => new()
    {
        Provider = spec.Provider,
        Endpoint = spec.Endpoint,
        Region = spec.Region,
        Bucket = spec.Bucket,
        PathStyle = spec.PathStyle,
        BlobServiceUrl = spec.BlobServiceUrl,
        VolumePath = UnderstudyJobEnvironment.VolumePath,
        AccessKey = environment.GetValueOrDefault(prefix + UnderstudyJobEnvironment.AccessKey) ?? string.Empty,
        SecretKey = environment.GetValueOrDefault(prefix + UnderstudyJobEnvironment.SecretKey) ?? string.Empty,
        AccountName = environment.GetValueOrDefault(prefix + UnderstudyJobEnvironment.AccountName) ?? string.Empty,
        AccountKey = environment.GetValueOrDefault(prefix + UnderstudyJobEnvironment.AccountKey) ?? string.Empty
    };

    static string Required(IReadOnlyDictionary<string, string> environment, string name) =>
        environment.TryGetValue(name, out var value) && !string.IsNullOrEmpty(value)
            ? value
            : throw new InvalidOperationException($"The environment variable {name} is not set.");

    static async Task<int> BackupAsync(
        UnderstudyJobConfiguration configuration,
        IReadOnlyDictionary<string, string> environment,
        IObjectStorageFactory factory,
        IChronicleDirectory directory,
        string work,
        ILoggerFactory loggers,
        CancellationToken cancellationToken)
    {
        var connection = Required(environment, UnderstudyJobEnvironment.MongoDBConnection);
        var storage = factory.Create(SettingsOf(configuration.Storage, UnderstudyJobEnvironment.StoragePrefix, environment));
        try
        {
            var job = new DataBackupJob(
                storage,
                directory,
                new MongoDatabaseCatalog(connection),
                new MongoDumpTool(connection, work),
                TimeProvider.System,
                work,
                loggers.CreateLogger<DataBackupJob>());
            return (await job.RunAsync(configuration, cancellationToken)).Succeeded ? Succeeded : Failed;
        }
        finally
        {
            (storage as IDisposable)?.Dispose();
        }
    }

    static async Task<int> KeysAsync(
        string kind,
        UnderstudyJobConfiguration configuration,
        IReadOnlyDictionary<string, string> environment,
        IObjectStorageFactory factory,
        IChronicleDirectory directory,
        ILoggerFactory loggers,
        CancellationToken cancellationToken)
    {
        var spec = configuration.KeysStorage ?? throw new InvalidOperationException("The job has no keys bucket.");
        using var certificate = KeyEncryptor.CertificateFromPem(Required(environment, UnderstudyJobEnvironment.KeysCertificate));
        using var vault = new VaultHttpKeyReader(new() { BaseAddress = new Uri(configuration.VaultAddress) }, Required(environment, UnderstudyJobEnvironment.VaultToken));
        var storage = factory.Create(SettingsOf(spec, UnderstudyJobEnvironment.KeysStoragePrefix, environment));
        try
        {
            var job = new KeyBackupJob(storage, directory, vault, certificate, TimeProvider.System, loggers.CreateLogger<KeyBackupJob>());
            var result = kind == UnderstudyJobKinds.VaultSnapshot
                ? await job.SnapshotAsync(configuration, cancellationToken)
                : await job.RunAsync(configuration, cancellationToken);
            if (result.Refused)
            {
                return Refused;
            }

            return result.Succeeded ? Succeeded : Failed;
        }
        finally
        {
            (storage as IDisposable)?.Dispose();
        }
    }
}
