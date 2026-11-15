// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using Cratis.Showrunner.StageManager.Understudies.Catalog;
using Cratis.Showrunner.StageManager.Understudies.Discovery;
using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies.Jobs;

/// <summary>
/// Takes the database backups of a Production and prunes. It asks Chronicle which event stores and namespaces exist, plans what to back up, and writes one run per
/// scope: the files first and the manifest last, so a run with a manifest is complete. A scope that fails leaves nothing behind and fails the whole run.
/// </summary>
/// <param name="storage">The storage of the database backups.</param>
/// <param name="directory">The Chronicle directory.</param>
/// <param name="databases">The database catalog.</param>
/// <param name="dump">The dump tool.</param>
/// <param name="time">The time provider.</param>
/// <param name="workDirectory">The directory archives are written to before they are uploaded.</param>
/// <param name="logger">The logger.</param>
public class DataBackupJob(
    IObjectStorage storage,
    IChronicleDirectory directory,
    IDatabaseCatalog databases,
    IDumpTool dump,
    TimeProvider time,
    string workDirectory,
    ILogger<DataBackupJob> logger)
{
    /// <summary>
    /// Gets the name of the file a planned backup is written to.
    /// </summary>
    /// <param name="backup">The planned backup.</param>
    /// <returns>The file name.</returns>
    public static string FileNameOf(PlannedBackup backup) => backup.Type == BackupType.Database
        ? $"Database-{backup.Database}.archive.gz"
        : $"{backup.Type}.archive.gz";

    /// <summary>
    /// Runs the job.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>What the run did.</returns>
    public async Task<BackupRunResult> RunAsync(UnderstudyJobConfiguration configuration, CancellationToken cancellationToken)
    {
        var takenAt = Truncate(time.GetUtcNow());
        var topology = await directory.GetTopologyAsync(cancellationToken);
        var existing = await databases.ListAsync(cancellationToken);
        var options = new BackupPlanOptions(new ChronicleDatabaseNames(configuration.DatabasePrefix), configuration.Databases, configuration.HasVault);
        var plan = BackupPlanner.Plan(topology, existing, options);
        logger.PlanMade(configuration.Production, plan.Backups.Count(backup => backup.Present), plan.Unclaimed.Count);
        await using (var report = new MemoryStream(PlanReport.From(plan, takenAt).ToJson()))
        {
            await storage.PutAsync(PlanReport.KeyOf(configuration.Production), report, cancellationToken);
        }

        var taken = new List<BackupManifest>();
        var failed = new List<BackupFailure>();
        foreach (var scope in plan.Backups.Where(backup => backup.Present).GroupBy(backup => (backup.EventStore, backup.Namespace)))
        {
            try
            {
                taken.Add(await BackupScopeAsync(configuration, scope.Key.EventStore, scope.Key.Namespace, [.. scope], takenAt, cancellationToken));
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.ScopeFailed(configuration.Production, scope.Key.EventStore ?? BackupLayout.NotApplicable, scope.Key.Namespace ?? BackupLayout.NotApplicable, exception.Message);
                failed.Add(new(scope.Key.EventStore, scope.Key.Namespace, exception.Message));
            }
        }

        var pruned = await new BackupPruner(storage, new BackupCatalog(storage)).PruneAsync(configuration.Production, configuration.Retention, time.GetUtcNow(), cancellationToken);
        logger.Pruned(configuration.Production, pruned.Kept, pruned.Pruned, pruned.Orphans);
        return new(taken, failed, pruned, plan);
    }

    static DateTimeOffset Truncate(DateTimeOffset moment) => new(moment.Year, moment.Month, moment.Day, moment.Hour, moment.Minute, moment.Second, TimeSpan.Zero);

    async Task<BackupManifest> BackupScopeAsync(
        UnderstudyJobConfiguration configuration,
        string? eventStore,
        string? @namespace,
        IReadOnlyList<PlannedBackup> backups,
        DateTimeOffset takenAt,
        CancellationToken cancellationToken)
    {
        var runPrefix = BackupLayout.RunPrefix(configuration.Production, eventStore, @namespace, takenAt);
        var written = new List<string>();
        try
        {
            var entries = new List<ManifestEntry>();
            foreach (var backup in backups)
            {
                var file = FileNameOf(backup);
                var archive = Path.Combine(workDirectory, $"{Guid.NewGuid():N}.archive.gz");
                try
                {
                    await dump.DumpAsync(backup.Database, archive, cancellationToken);
                    await using var content = File.OpenRead(archive);
                    var checksum = Convert.ToHexStringLower(await SHA256.HashDataAsync(content, cancellationToken));
                    content.Position = 0;
                    await storage.PutAsync(runPrefix + file, content, cancellationToken);
                    written.Add(runPrefix + file);
                    entries.Add(new(backup.Type, backup.Database, file, content.Length, checksum));
                }
                finally
                {
                    File.Delete(archive);
                }
            }

            var manifest = new BackupManifest(BackupManifest.CurrentSchemaVersion, configuration.Production, eventStore, @namespace, takenAt, configuration.ChronicleVersion, configuration.Retention, entries);
            await using var json = new MemoryStream(manifest.ToJson());
            await storage.PutAsync(BackupLayout.ManifestKey(runPrefix), json, cancellationToken);
            logger.ScopeBackedUp(configuration.Production, eventStore ?? BackupLayout.NotApplicable, @namespace ?? BackupLayout.NotApplicable, entries.Count, entries.Sum(entry => entry.SizeBytes));
            return manifest;
        }
        catch
        {
            // Never success with partial output: what the run wrote is removed, and the manifest was never written.
            foreach (var key in written)
            {
                await storage.DeleteAsync(key, CancellationToken.None);
            }

            throw;
        }
    }
}
