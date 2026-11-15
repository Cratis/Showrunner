// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageManager.Understudies.Catalog;
using Cratis.Showrunner.Understudy.Contracts.Backups;

namespace Cratis.Showrunner.StageManager.Understudies.Reporting;

/// <summary>
/// Reports what happened to backups: publishes the public facts to every sink and counts them. A fact is published once the operator has seen the job finish,
/// and it is published under an id taken from the job or the backup, so the operator restarting and seeing the same job again repeats nothing a consumer would count twice.
/// </summary>
/// <param name="sinks">Where facts are published.</param>
/// <param name="metrics">The metrics.</param>
public class UnderstudyReporter(IEnumerable<IUnderstudyEventSink> sinks, UnderstudyMetrics metrics)
{
    /// <summary>
    /// Gets the id a backup run has in the contracts: its place in the catalog without the manifest file.
    /// </summary>
    /// <param name="entry">The run.</param>
    /// <returns>The id.</returns>
    public static string BackupIdOf(CatalogEntry entry) => entry.RunPrefix.TrimEnd('/');

    /// <summary>
    /// Reports that a backup run was taken.
    /// </summary>
    /// <param name="understudy">The Understudy.</param>
    /// <param name="entry">The run.</param>
    /// <param name="kind">The kind of job that took it.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the fact is published.</returns>
    public async Task BackupTakenAsync(Understudy understudy, CatalogEntry entry, string kind, CancellationToken cancellationToken)
    {
        var manifest = entry.Manifest;
        var id = BackupIdOf(entry);
        var payload = new BackupTaken(
            manifest.Production,
            id,
            manifest.EventStore ?? string.Empty,
            manifest.Namespace ?? string.Empty,
            manifest.TakenAt,
            manifest.Entries.Select(item => item.Type.ToString()).Distinct(StringComparer.Ordinal).ToList(),
            manifest.Entries.Sum(item => item.SizeBytes));

        await PublishAsync(understudy, new($"BackupTaken:{id}", nameof(BackupTaken), payload, false), cancellationToken);
        metrics.CountRun(manifest.Production, kind, "taken");
    }

    /// <summary>
    /// Reports that a backup job failed.
    /// </summary>
    /// <param name="understudy">The Understudy.</param>
    /// <param name="production">The name of the Production.</param>
    /// <param name="jobName">The job that failed.</param>
    /// <param name="kind">The kind of job.</param>
    /// <param name="failedAt">When it failed.</param>
    /// <param name="reason">Why it failed.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the fact is published.</returns>
    public async Task BackupFailedAsync(Understudy understudy, string production, string jobName, string kind, DateTimeOffset failedAt, string reason, CancellationToken cancellationToken)
    {
        var payload = new BackupFailed(production, jobName, failedAt, kind, reason);
        await PublishAsync(understudy, new($"BackupFailed:{jobName}", nameof(BackupFailed), payload, true), cancellationToken);
        metrics.CountRun(production, kind, "failed");
    }

    /// <summary>
    /// Records what the catalog holds for the metrics.
    /// </summary>
    /// <param name="production">The Production.</param>
    /// <param name="lastBackups">The latest backup of each type.</param>
    /// <param name="healthy">Whether the latest run succeeded.</param>
    /// <param name="unclaimed">The number of databases no rule claims.</param>
    public void Observe(string production, IEnumerable<LastBackupStatus> lastBackups, bool healthy, int unclaimed) =>
        metrics.Observe(production, lastBackups, healthy, unclaimed);

    async Task PublishAsync(Understudy understudy, UnderstudyFact fact, CancellationToken cancellationToken)
    {
        foreach (var sink in sinks)
        {
            await sink.PublishAsync(understudy, fact, cancellationToken);
        }
    }
}
