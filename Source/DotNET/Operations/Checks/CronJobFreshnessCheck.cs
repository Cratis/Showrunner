// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks;

/// <summary>
/// Finds CronJobs that have stopped succeeding. This is how a database backup that silently stopped, or another watchdog that
/// failed every pass for two days while its CronJob history was the only sign, is noticed: silence reads as health.
/// </summary>
/// <param name="reader">The cluster reader.</param>
/// <param name="expectations">The CronJobs that must keep succeeding.</param>
public sealed class CronJobFreshnessCheck(IClusterReader reader, IEnumerable<CronJobExpectation> expectations) : IWatchdogCheck
{
    /// <summary>
    /// The name of the check in configuration.
    /// </summary>
    public const string CheckName = "cronjobs";

    readonly IReadOnlyList<CronJobExpectation> _expectations = [.. expectations];

    /// <inheritdoc/>
    public string Name => CheckName;

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Finding>> Collect(CheckContext context, CancellationToken cancellationToken)
    {
        var findings = new List<Finding>();
        foreach (var expectation in _expectations)
        {
            if (await Look(expectation, context.Now, cancellationToken) is { } finding)
            {
                findings.Add(finding);
            }
        }

        return findings;
    }

    async Task<Finding?> Look(CronJobExpectation expectation, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var name = expectation.Name;
        CronJobInfo? cronJob;
        try
        {
            cronJob = await reader.GetCronJob(expectation.Namespace, name, cancellationToken);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            return new Finding($"{name}:api", $"could not read {name} freshness: {Redaction.Summarize(error.Message)}; inspect watchdog RBAC and the CronJob", AlertSeverity.Warning);
        }

        if (cronJob is null)
        {
            return expectation.Optional
                ? null
                : new Finding($"{name}:api", $"could not read {name} freshness: the CronJob {expectation.Namespace}/{name} does not exist; inspect the deployment", AlertSeverity.Warning);
        }

        var stamp = cronJob.LastSuccessful ?? cronJob.Created;
        if (stamp is not { } reference)
        {
            return new Finding($"{name}:api", $"{name} status is unreadable; inspect the CronJob", AlertSeverity.Warning);
        }

        var age = now - reference;
        return age > expectation.MaximumAge
            ? new Finding(
                $"{name}:stale",
                $"{name} has not succeeded for {(int)age.TotalMinutes} minutes (last success {reference.UtcDateTime:O}); inspect the CronJob and its failed Jobs",
                AlertSeverity.Critical)
            : null;
    }
}
