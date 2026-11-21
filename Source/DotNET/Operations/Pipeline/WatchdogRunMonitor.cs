// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.Cluster;

namespace Cratis.Showrunner.Operations.Pipeline;

/// <summary>
/// Watches the watchdog itself: a gap of three schedule intervals since the last run, and a failed Job of the watchdog's own
/// CronJob, reported once. A watchdog that stops running reports nothing, and that silence reads as health.
/// </summary>
/// <param name="reader">The cluster reader.</param>
/// <param name="options">The options.</param>
public sealed class WatchdogRunMonitor(IClusterReader reader, WatchdogOptions options)
{
    /// <summary>
    /// The prefix of the key of a failed-Job finding.
    /// </summary>
    public const string FailedRunPrefix = "watchdog:failed-run:";

    /// <summary>
    /// Looks at the watchdog's own runs.
    /// </summary>
    /// <param name="started">When this run started.</param>
    /// <param name="lastCheck">When the previous run started.</param>
    /// <param name="reported">The failed Jobs that were already reported.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The findings and the failed Jobs that are reported and still retained.</returns>
    public async Task<(IReadOnlyList<Finding> Findings, ISet<string> Reported)> Check(
        DateTimeOffset started,
        DateTimeOffset lastCheck,
        IReadOnlyCollection<string> reported,
        CancellationToken cancellationToken)
    {
        var findings = new List<Finding>();
        if (options.ScheduleInterval is { } interval && started - lastCheck > interval * 3)
        {
            findings.Add(new Finding("watchdog:missed-runs", $"watchdog missed runs: {(int)(started - lastCheck).TotalSeconds}s since last check", AlertSeverity.Warning));
        }

        if (string.IsNullOrEmpty(options.WatchdogCronJob))
        {
            return (findings, new HashSet<string>(reported));
        }

        IReadOnlyList<JobInfo> jobs;
        try
        {
            jobs = await reader.ListJobs(options.Namespace, cancellationToken);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            findings.Add(new Finding("watchdog:jobs-api", $"could not list watchdog Jobs: {Redaction.Summarize(error.Message)}", AlertSeverity.Warning));
            return (findings, new HashSet<string>(reported));
        }

        var retained = new HashSet<string>();
        foreach (var job in jobs.Where(job => job.OwnerCronJobs.Contains(options.WatchdogCronJob)))
        {
            retained.Add(job.Name);
            if (job.Failed && !reported.Contains(job.Name))
            {
                findings.Add(new Finding(
                    FailedRunPrefix + job.Name,
                    $"watchdog Job {job.Name} failed; started {job.Started?.UtcDateTime.ToString("O") ?? "unknown"}: {job.FailureReason ?? "unknown"}: {job.FailureMessage}",
                    AlertSeverity.Critical));
            }
        }

        return (findings, new HashSet<string>(reported.Where(retained.Contains)));
    }
}
