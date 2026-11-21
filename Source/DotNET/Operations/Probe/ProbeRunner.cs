// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using Cratis.Showrunner.Operations.Alerts;

namespace Cratis.Showrunner.Operations.Probe;

/// <summary>
/// The external probe job: probes the public endpoints, writes a Markdown summary, and tells every sink that is configured. It runs
/// outside the cluster, from CI, because a probe inside the cluster cannot see an ingress or load balancer that is down. It keeps no
/// state: every run reports what it sees, so the sinks that need history (the outage issue, Healthchecks) keep it themselves.
/// A sink that fails never hides the result or the other sinks, and what a sink failure says is only which sink it was.
/// </summary>
/// <param name="evaluator">The probe evaluator.</param>
/// <param name="delivery">Delivers an alert to the shared channels.</param>
/// <param name="healthchecks">The Healthchecks sink, or <see langword="null"/> when disabled.</param>
/// <param name="issues">The GitHub issue sink, or <see langword="null"/> when disabled.</param>
/// <param name="deployment">The deployment name shown in alerts.</param>
/// <param name="log">Where the one-line sink results go; they carry no address or credential.</param>
public sealed class ProbeRunner(
    ProbeEvaluator evaluator,
    AlertDelivery delivery,
    HealthchecksProbeSink? healthchecks,
    GitHubIssueProbeSink? issues,
    string deployment,
    Action<string> log)
{
    /// <summary>
    /// Gets the Markdown summary of a report, for the job summary of a CI run.
    /// </summary>
    /// <param name="report">The report.</param>
    /// <returns>The Markdown.</returns>
    public static string Summary(ProbeReport report)
    {
        var rows = report.Rounds.SelectMany(round => round).Select(result => $"| {result.Target}/{result.Check} | {result.Status?.ToString(CultureInfo.InvariantCulture) ?? result.Error} | {(result.Ok ? "ok" : "failed")} |");
        var flaky = report.Flaky.Count == 0 ? "none" : string.Join(", ", report.Flaky.Select(result => $"{result.Target}/{result.Check}"));
        return string.Join(
            '\n',
            [$"## Public endpoint probe: {(report.IsDown ? "down" : "ok")}", string.Empty, "| Check | Status | Result |", "| --- | --- | --- |", .. rows, string.Empty, $"flaky: {flaky}", string.Empty]);
    }

    /// <summary>
    /// Probes, reports and returns the exit code.
    /// </summary>
    /// <param name="targets">The targets.</param>
    /// <param name="roundGap">The pause before the second round.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The process exit code: 1 when an endpoint is down, 0 otherwise.</returns>
    public async Task<(int ExitCode, string Summary)> Run(IReadOnlyList<ProbeTarget> targets, TimeSpan roundGap, CancellationToken cancellationToken)
    {
        var report = await evaluator.Evaluate(targets, roundGap, cancellationToken);
        var summary = Summary(report);
        log(summary);
        await Task.WhenAll(Alert(report, cancellationToken), Sink("healthchecks", healthchecks is null ? null : token => healthchecks.Report(report, token), cancellationToken), Sink("issues", issues is null ? null : token => issues.Report(report, token), cancellationToken));
        return (report.IsDown ? 1 : 0, summary);
    }

    async Task Alert(ProbeReport report, CancellationToken cancellationToken)
    {
        if (!report.IsDown)
        {
            return;
        }

        var findings = report.Failing.Select(result => new Finding($"probe:{result.Target}/{result.Check}", $"public endpoint {result.Description}")).ToList();
        try
        {
            var result = await delivery.Deliver(new AlertNotification(deployment, findings, [], [], []), cancellationToken);
            log(result.AnyDelivered ? "alerts: delivered" : "alerts: none delivered");
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            log("alerts: failed");
        }
    }

    async Task Sink(string name, Func<CancellationToken, Task>? deliver, CancellationToken cancellationToken)
    {
        if (deliver is null)
        {
            log($"{name}: disabled");
            return;
        }

        try
        {
            await deliver(cancellationToken);
            log($"{name}: delivered");
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            log($"{name}: failed");
        }
    }
}
