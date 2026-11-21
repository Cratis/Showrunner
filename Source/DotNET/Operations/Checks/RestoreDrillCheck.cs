// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks;

/// <summary>
/// Watches the restore drill, the CronJob that proves a backup can be restored. It runs once a day, so a missed or failed run is
/// told apart from "not due yet": thirty hours is a day plus a window for a slow run, and the newest finished Job decides
/// whether the last run failed.
/// </summary>
/// <param name="reader">The cluster reader.</param>
/// <param name="namespace">The namespace the drill runs in.</param>
/// <param name="cronJobName">The name of the drill CronJob.</param>
/// <param name="staleAfter">How long the drill may go without succeeding.</param>
public sealed class RestoreDrillCheck(IClusterReader reader, string @namespace, string cronJobName, TimeSpan staleAfter) : IWatchdogCheck
{
    /// <summary>
    /// The name of the check in configuration.
    /// </summary>
    public const string CheckName = "restore-drill";

    /// <inheritdoc/>
    public string Name => CheckName;

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Finding>> Collect(CheckContext context, CancellationToken cancellationToken)
    {
        CronJobInfo? cronJob;
        try
        {
            cronJob = await reader.GetCronJob(@namespace, cronJobName, cancellationToken);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            var status = error is ClusterReadException { StatusCode: { } code } ? $"HTTP {code}" : error.GetType().Name;
            return [new Finding("restore-drill:api", $"could not read the {cronJobName} CronJob ({status}); inspect watchdog RBAC", AlertSeverity.Warning)];
        }

        if (cronJob is null)
        {
            return [new Finding("restore-drill:missing", $"the {cronJobName} CronJob does not exist although the drill is enabled; run the deploy", AlertSeverity.Critical)];
        }

        var findings = new List<Finding>();
        try
        {
            var reference = cronJob.LastSuccessful ?? cronJob.Created ?? throw new InvalidOperationException("The CronJob has no timestamps.");
            var age = context.Now - reference;
            if (age > staleAfter)
            {
                findings.Add(new Finding(
                    "restore-drill:stale",
                    $"the restore drill has not succeeded for {(int)age.TotalHours} hours (last success {reference.UtcDateTime:O}); check that the CronJob is not suspended and read its last Job's logs",
                    AlertSeverity.Critical));
            }

            var newest = (await reader.ListJobs(@namespace, cancellationToken))
                .Where(job => job.OwnerCronJobs.Contains(cronJobName) && (job.Failed || job.Complete))
                .OrderByDescending(job => job.Created)
                .FirstOrDefault();
            if (newest is { Failed: true })
            {
                findings.Add(new Finding(
                    "restore-drill:failed",
                    $"the last restore drill Job {newest.Name} failed ({Redaction.Summarize(newest.FailureReason ?? "unknown")}): {await TerminationMessage(newest, cancellationToken)}; read its logs for the RESULT and cmp lines",
                    AlertSeverity.Critical));
            }
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            return [new Finding("restore-drill:api", $"could not read the restore drill state ({error.GetType().Name}); inspect watchdog RBAC and the CronJob", AlertSeverity.Warning)];
        }

        return findings;
    }

    /// <summary>
    /// Reads why the drill Job failed: the report container's termination message holds only the RESULT line, the snapshot and
    /// the phase reasons.
    /// </summary>
    /// <param name="job">The failed Job.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The redacted message, or a note that there is none.</returns>
    async Task<string> TerminationMessage(JobInfo job, CancellationToken cancellationToken)
    {
        try
        {
            var pods = await reader.ListPods(@namespace, $"job-name={job.Name}", cancellationToken);
            var message = pods.SelectMany(pod => pod.Containers).Select(container => container.TerminationMessage).FirstOrDefault(text => !string.IsNullOrEmpty(text));
            if (message is not null)
            {
                return Redaction.Summarize(message);
            }
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            // The Job's own reason is still reported.
        }

        return "no termination message";
    }
}
