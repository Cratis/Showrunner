// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Operations.Alerts;

namespace Cratis.Showrunner.Operations.Pipeline;

/// <summary>
/// Runs every check under its own time budget and the overall collection deadline. A check that throws or runs out of time
/// becomes a finding of its own; it never aborts the run and never hides the findings of the others.
/// </summary>
/// <param name="checks">The enabled checks.</param>
/// <param name="options">The timing options.</param>
/// <param name="timeProvider">The clock.</param>
public sealed class FindingCollector(IEnumerable<IWatchdogCheck> checks, WatchdogOptions options, TimeProvider timeProvider)
{
    readonly IReadOnlyList<IWatchdogCheck> _checks = [.. checks];

    /// <summary>
    /// Hands the saved state to the check that keeps state of its own.
    /// </summary>
    /// <param name="json">The saved state, or <see langword="null"/>.</param>
    public void Restore(string? json)
    {
        foreach (var check in _checks.OfType<IPersistentCheck>())
        {
            check.Restore(json);
        }
    }

    /// <summary>
    /// Gets the state the persistent check wants saved.
    /// </summary>
    /// <param name="previous">The state that was loaded, kept when no check has state of its own.</param>
    /// <returns>The state to save.</returns>
    public string? Persist(string? previous) =>
        _checks.OfType<IPersistentCheck>().Select(check => check.Persist()).FirstOrDefault(state => state is not null) ?? previous;

    /// <summary>
    /// Runs all checks once.
    /// </summary>
    /// <param name="windowStarted">When the previous run started.</param>
    /// <param name="collectionDeadline">When collection must be finished.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The findings, by key. A later finding of the same key replaces an earlier one.</returns>
    public async Task<IReadOnlyDictionary<string, Finding>> Collect(DateTimeOffset windowStarted, DateTimeOffset collectionDeadline, CancellationToken cancellationToken)
    {
        var findings = new Dictionary<string, Finding>(StringComparer.Ordinal);
        foreach (var check in _checks)
        {
            foreach (var finding in await Run(check, windowStarted, collectionDeadline, cancellationToken))
            {
                findings[finding.Key] = finding;
            }
        }

        return findings;
    }

    async Task<IReadOnlyCollection<Finding>> Run(IWatchdogCheck check, DateTimeOffset windowStarted, DateTimeOffset collectionDeadline, CancellationToken cancellationToken)
    {
        var remaining = collectionDeadline - timeProvider.GetUtcNow();
        if (remaining <= TimeSpan.Zero)
        {
            return [new Finding($"watchdog:budget:{check.Name}", $"check '{check.Name}' did not run: collection budget exhausted", AlertSeverity.Warning)];
        }

        var budget = remaining < options.CheckBudget ? remaining : options.CheckBudget;
        using var budgetSource = new CancellationTokenSource(budget, timeProvider);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, budgetSource.Token);
        try
        {
            var context = new CheckContext(timeProvider.GetUtcNow(), windowStarted, collectionDeadline, timeProvider);
            return await check.Collect(context, linked.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return [new Finding($"watchdog:check:{check.Name}", $"check '{check.Name}' did not finish within {(int)budget.TotalSeconds}s; it is not reporting", AlertSeverity.Warning)];
        }
        catch (Exception error)
        {
            return [new Finding($"watchdog:check:{check.Name}", $"check '{check.Name}' failed: {Redaction.Summarize(error.Message)}", AlertSeverity.Warning)];
        }
    }
}
