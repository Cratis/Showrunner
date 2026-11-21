// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks;

/// <summary>
/// Finds Percona Backup for MongoDB that has no ready backup within the allowed age, and says why when a backup failed. The
/// error a backup recorded can carry connection strings, so it is redacted and truncated.
/// </summary>
/// <param name="reader">The cluster reader.</param>
/// <param name="namespace">The namespace the backups live in.</param>
/// <param name="maximumAge">How old the newest ready backup may be.</param>
public sealed class PbmBackupsCheck(IClusterReader reader, string @namespace, TimeSpan maximumAge) : IWatchdogCheck
{
    /// <summary>
    /// The name of the check in configuration.
    /// </summary>
    public const string CheckName = "pbm-backups";

    /// <inheritdoc/>
    public string Name => CheckName;

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Finding>> Collect(CheckContext context, CancellationToken cancellationToken)
    {
        IReadOnlyList<PbmBackupInfo> backups;
        try
        {
            backups = await reader.ListPbmBackups(@namespace, cancellationToken);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            return [new Finding("pbm-backups:api", $"could not read PBM backup freshness ({error.GetType().Name}); inspect watchdog RBAC and Percona backup status", AlertSeverity.Warning)];
        }

        // The operator records successful completion in status.completed, not in the creation time.
        var ready = backups.Where(backup => backup.State == "ready" && backup.Completed is not null).Select(backup => backup.Completed!.Value).ToList();
        if (ready.Count > 0 && context.Now - ready.Max() <= maximumAge)
        {
            return [];
        }

        var key = ready.Count > 0 ? "stale" : "missing";
        var message = ready.Count > 0 ? $"PBM has no ready backup in over {(int)maximumAge.TotalHours} hours" : "PBM has no ready backup";
        var newestFailed = backups.Where(backup => backup.State == "error").OrderByDescending(backup => backup.Created).FirstOrDefault();
        if (newestFailed is not null)
        {
            message += $"; newest failed backup {newestFailed.Name}: {Redaction.Summarize(newestFailed.Error ?? "no error detail")}";
        }

        return [new Finding($"pbm-backups:{key}", message + "; inspect the Percona backup and PBM agent logs", AlertSeverity.Critical)];
    }
}
