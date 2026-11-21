// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.Checks;
using Cratis.Showrunner.Operations.Link;
using Cratis.Showrunner.Operations.State;

namespace Cratis.Showrunner.Operations.Pipeline;

/// <summary>
/// One pass of the watchdog: collect findings from the checks, confirm them with a re-check, decide what is new, due again,
/// escalated or resolved, deliver one alert to every channel, remember it, and ping the heartbeat. A run is created per pass.
/// </summary>
/// <param name="collector">Runs the checks.</param>
/// <param name="monitor">Watches the watchdog's own runs.</param>
/// <param name="state">Where cooldowns and first-seen times are kept.</param>
/// <param name="delivery">Delivers to every channel.</param>
/// <param name="heartbeat">The external heartbeat, or <see langword="null"/> when none is configured.</param>
/// <param name="options">The timing options.</param>
/// <param name="timeProvider">The clock.</param>
/// <param name="logger">The logger.</param>
/// <param name="link">The optional link to Cratis Direct, or <see langword="null"/> when the watchdog only alerts through its channels.</param>
public sealed class WatchdogRun(
    FindingCollector collector,
    WatchdogRunMonitor monitor,
    IAlertState state,
    AlertDelivery delivery,
    IHeartbeat? heartbeat,
    WatchdogOptions options,
    TimeProvider timeProvider,
    ILogger<WatchdogRun> logger,
    IWatchdogLink? link = null)
{
    const int RememberedFailedJobs = 10;

    readonly AlertPolicy _policy = new(options);
    bool _stateOutageAlerted;

    /// <summary>
    /// Runs one pass. A failure anywhere is reported (an alert under its own cooldown, the Job marked reported, a failed
    /// heartbeat) and returned as the outcome with the original exception; it is never thrown.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>How the run ended.</returns>
    public async Task<WatchdogOutcome> Run(CancellationToken cancellationToken)
    {
        var started = timeProvider.GetUtcNow();
        try
        {
            if (link is not null)
            {
                await link.Begin(cancellationToken);
            }

            await Execute(started, cancellationToken);
            return new WatchdogOutcome();
        }
        catch (Exception error)
        {
            logger.RunFailed(error.GetType().Name);
            await Swallow(() => ReportRunFailure(error, cancellationToken));
            await Swallow(() => Ping(true, cancellationToken));
            return new WatchdogOutcome(error);
        }
        finally
        {
            if (link is not null)
            {
                // The state must be saved even when the pass was cancelled, within a budget of its own.
                using var ending = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                await Swallow(() => link.End(ending.Token));
            }
        }
    }

    /// <summary>
    /// Delivers a test alert to every channel. It reads and writes no state, does not ping the heartbeat and does not claim
    /// that health checks ran; one failing channel fails the test, which a real alert does not.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>How the test ended.</returns>
    public async Task<WatchdogOutcome> RunTestAlert(CancellationToken cancellationToken)
    {
        var finding = new Finding("test", "TEST ALERT: operator-requested delivery verification; no incident", AlertSeverity.Information);
        var notification = new AlertNotification(options.DeploymentName, [finding], [], [], [], IsTest: true);
        try
        {
            var result = await delivery.Deliver(notification, cancellationToken);
            return result.AnyDelivered && result.Failed.Count == 0 ? new WatchdogOutcome() : new WatchdogOutcome(DeliveryFailure(result));
        }
        catch (Exception error)
        {
            return new WatchdogOutcome(error);
        }
    }

    static InvalidOperationException DeliveryFailure(DeliveryResult result) =>
        new("alert delivery failed: " + (result.Failed.Count == 0 ? "none configured" : string.Join("; ", result.Failed.Select(name => $"{name}: failed"))));

    static Dictionary<string, Finding> Merge(IReadOnlyDictionary<string, Finding> first, IReadOnlyDictionary<string, Finding> second)
    {
        var merged = new Dictionary<string, Finding>(first, StringComparer.Ordinal);
        foreach (var (key, finding) in second)
        {
            merged[key] = finding;
        }

        return merged;
    }

    /// <summary>
    /// A legacy state entry only carries when the finding was last alerted, which is the best first-seen time there is.
    /// </summary>
    /// <param name="snapshot">The state.</param>
    /// <param name="key">The finding key.</param>
    /// <param name="now">The current time.</param>
    /// <returns>When the finding was first seen.</returns>
    static DateTimeOffset FirstSeenOf(AlertStateSnapshot snapshot, string key, DateTimeOffset now)
    {
        if (snapshot.FirstSeen.TryGetValue(key, out var seen))
        {
            return seen;
        }

        return snapshot.LastAlerted.TryGetValue(key, out var alerted) ? alerted : now;
    }

    /// <summary>
    /// A batch node's unreadable summary is held back until it has failed several consecutive runs.
    /// </summary>
    /// <param name="findings">The confirmed findings; held-back ones are removed.</param>
    /// <param name="previous">The consecutive failed runs of the previous run.</param>
    /// <returns>The consecutive failed runs including this one.</returns>
    static ImmutableDictionary<string, int> ApplySummaryStreaks(Dictionary<string, Finding> findings, ImmutableDictionary<string, int> previous)
    {
        var streaks = findings.Keys
            .Where(key => key.StartsWith(VolumesCheck.BatchSummaryPrefix, StringComparison.Ordinal))
            .ToImmutableDictionary(key => key, key => (previous.TryGetValue(key, out var count) ? count : 0) + 1, StringComparer.Ordinal);
        foreach (var key in streaks.Where(entry => entry.Value < VolumesCheck.BatchSummaryFailureRuns).Select(entry => entry.Key))
        {
            findings.Remove(key);
        }

        return streaks;
    }

    async Task Swallow(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception error)
        {
            // Reporting a failure must never hide the failure being reported.
            logger.SecondaryFailure(error.GetType().Name);
        }
    }

    async Task Execute(DateTimeOffset started, CancellationToken cancellationToken)
    {
        var fixedFindings = new Dictionary<string, Finding>(StringComparer.Ordinal);
        AlertStateSnapshot snapshot;
        try
        {
            snapshot = await state.Load(cancellationToken);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            // Missing cooldown state must not prevent alerting.
            snapshot = AlertStateSnapshot.Empty;
            fixedFindings["state:api"] = new Finding("state:api", $"could not load watchdog cooldown state: {Redaction.Summarize(error.Message)}; inspect watchdog API access", AlertSeverity.Warning);
        }

        collector.Restore(snapshot.ActionsAudit);
        var windowStarted = snapshot.LastCheck ?? (started - (options.ScheduleInterval ?? TimeSpan.FromMinutes(5)));
        var (runFindings, reported) = await monitor.Check(started, windowStarted, snapshot.ReportedFailedJobs, cancellationToken);
        fixedFindings = Merge(fixedFindings, runFindings.ToDictionary(finding => finding.Key, StringComparer.Ordinal));

        var findings = await Confirm(fixedFindings, started, windowStarted, cancellationToken);
        if (link is not null)
        {
            await link.Observe(findings, cancellationToken);
        }

        var summaryStreaks = ApplySummaryStreaks(findings, snapshot.SummaryStreaks);
        var now = timeProvider.GetUtcNow();
        var firstSeen = findings.Keys.ToImmutableDictionary(
            key => key,
            key => FirstSeenOf(snapshot, key, now),
            StringComparer.Ordinal);
        var lastAlerted = snapshot.LastAlerted;

        var due = findings.Keys.Where(key => _policy.IsDue(now, snapshot.LastAlerted.TryGetValue(key, out var at) ? at : null, firstSeen[key])).ToHashSet(StringComparer.Ordinal);
        var escalated = findings.Keys.Where(key => _policy.IsEscalated(now, firstSeen[key])).ToHashSet(StringComparer.Ordinal);
        var shown = findings.ToDictionary(entry => entry.Key, entry => entry.Value with { Text = AlertPolicy.WithOpenSince(entry.Value.Text, firstSeen[entry.Key], now) }, StringComparer.Ordinal);
        var resolved = snapshot.FirstSeen.Keys.Concat(snapshot.LastAlerted.Keys)
            .Where(key => key != AlertStateSnapshot.RunFailedKey && !findings.ContainsKey(key))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();

        if (resolved.Count > 0)
        {
            logger.Resolved(resolved.Count);
        }

        if (shown.Count == 0)
        {
            logger.Healthy();
        }
        else
        {
            logger.Findings(shown.Count, due.Count);
            foreach (var key in shown.Keys.Order(StringComparer.Ordinal))
            {
                logger.Finding(shown[key].Text);
            }

            if (due.Count > 0)
            {
                var notification = new AlertNotification(
                    options.DeploymentName,
                    [.. due.Except(escalated).Order(StringComparer.Ordinal).Select(key => shown[key])],
                    [.. escalated.Order(StringComparer.Ordinal).Select(key => shown[key])],
                    [.. shown.Keys.Except(due).Except(escalated).Order(StringComparer.Ordinal).Select(key => shown[key])],
                    resolved);
                var result = await delivery.Deliver(notification, cancellationToken);
                if (!result.AnyDelivered)
                {
                    throw DeliveryFailure(result);
                }

                lastAlerted = lastAlerted.SetItems(due.Select(key => KeyValuePair.Create(key, now)));
                if (due.Contains("state:api"))
                {
                    // This delivery already covers a state outage; do not alert again if saving fails.
                    lastAlerted = lastAlerted.SetItem(AlertStateSnapshot.RunFailedKey, now);
                    _stateOutageAlerted = true;
                }

                foreach (var job in shown.Keys.Where(key => key.StartsWith(WatchdogRunMonitor.FailedRunPrefix, StringComparison.Ordinal)))
                {
                    reported.Add(job[WatchdogRunMonitor.FailedRunPrefix.Length..]);
                }

                logger.AlertSent(string.Join(", ", result.Delivered));
            }
            else
            {
                logger.CoolingDown();
            }
        }

        // Drop resolved findings so a recurrence starts fresh, including its open-since time.
        lastAlerted = lastAlerted.Where(entry => findings.ContainsKey(entry.Key) || entry.Key == AlertStateSnapshot.RunFailedKey).ToImmutableDictionary(StringComparer.Ordinal);
        await state.Save(
            snapshot with
            {
                LastAlerted = lastAlerted,
                FirstSeen = firstSeen,
                SummaryStreaks = summaryStreaks,
                ActionsAudit = collector.Persist(snapshot.ActionsAudit),
                LastCheck = started,
                ReportedFailedJobs = [.. reported.Order(StringComparer.Ordinal)]
            },
            cancellationToken);
        await Ping(false, cancellationToken);
    }

    async Task<Dictionary<string, Finding>> Confirm(IReadOnlyDictionary<string, Finding> fixedFindings, DateTimeOffset started, DateTimeOffset windowStarted, CancellationToken cancellationToken)
    {
        var deadline = started + options.Deadline - options.DeliveryBudget;
        var firstPassStarted = timeProvider.GetUtcNow();
        var findings = Merge(await collector.Collect(windowStarted, deadline, cancellationToken), fixedFindings);
        var firstPass = timeProvider.GetUtcNow() - firstPassStarted;
        if (findings.Count == 0)
        {
            return findings;
        }

        // Only a completely clean re-check silences the first pass. Anything still failing means it did not see a healthy
        // cluster, so both passes are kept and the current text wins.
        var elapsed = timeProvider.GetUtcNow() - started;
        if (!ShouldRecheck(elapsed, firstPass))
        {
            logger.NoTimeToRecheck((int)elapsed.TotalSeconds);
            return findings;
        }

        logger.Rechecking(findings.Count);
        await Task.Delay(options.RecheckPause, timeProvider, cancellationToken);
        var confirmed = Merge(await collector.Collect(windowStarted, deadline, cancellationToken), fixedFindings);
        if (confirmed.Count > 0)
        {
            return Merge(findings, confirmed);
        }

        logger.RecheckClean();
        return [];
    }

    bool ShouldRecheck(TimeSpan elapsed, TimeSpan firstPass)
    {
        // The second pass takes about as long as the first did, so it only runs if the pause, another pass that long and the
        // alert delivery still fit within the deadline. Skipping it means alerting on the first pass, never silence.
        var projected = elapsed + options.RecheckPause + firstPass + options.DeliveryBudget;
        return elapsed <= options.Deadline - options.RecheckBudget && projected <= options.Deadline;
    }

    async Task ReportRunFailure(Exception error, CancellationToken cancellationToken)
    {
        // Never merge an earlier snapshot over state that the run may already have saved.
        var snapshot = AlertStateSnapshot.Empty;
        await Swallow(async () => snapshot = await state.Load(cancellationToken));

        var now = timeProvider.GetUtcNow();
        if (!snapshot.LastAlerted.TryGetValue(AlertStateSnapshot.RunFailedKey, out var last) || now - last > options.RealertInterval)
        {
            if (!_stateOutageAlerted)
            {
                var finding = new Finding(
                    AlertStateSnapshot.RunFailedKey,
                    $"watchdog run failed: {error.GetType().Name}: {Redaction.Summarize(error.Message)}",
                    AlertSeverity.Critical);
                var result = await delivery.Deliver(new AlertNotification(options.DeploymentName, [finding], [], [], []), cancellationToken);
                if (!result.AnyDelivered)
                {
                    throw DeliveryFailure(result);
                }
            }

            snapshot = snapshot with { LastAlerted = snapshot.LastAlerted.SetItem(AlertStateSnapshot.RunFailedKey, now) };
        }

        var reported = snapshot.ReportedFailedJobs;
        if (!string.IsNullOrEmpty(options.WatchdogJobName) && !reported.Contains(options.WatchdogJobName))
        {
            reported = reported.Add(options.WatchdogJobName);
        }

        // Pruning normally happens during collection; bound this fallback if collection itself fails.
        snapshot = snapshot with { ReportedFailedJobs = [.. reported.TakeLast(RememberedFailedJobs)] };
        await Swallow(() => state.Save(snapshot, cancellationToken));
    }

    async Task Ping(bool failed, CancellationToken cancellationToken)
    {
        if (heartbeat is null)
        {
            return;
        }

        await heartbeat.Ping(failed, cancellationToken);
        logger.HeartbeatSent(failed ? "failure reported" : "succeeded");
    }
}
