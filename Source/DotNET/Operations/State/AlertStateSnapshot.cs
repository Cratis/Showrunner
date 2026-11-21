// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.State;

/// <summary>
/// Everything the watchdog remembers between runs.
/// </summary>
/// <param name="LastAlerted">When each open finding was last alerted — the cooldown.</param>
/// <param name="FirstSeen">When each open finding was first seen — the escalation clock.</param>
/// <param name="LastCheck">When the previous run started, for missed-run detection and the eviction window.</param>
/// <param name="SummaryStreaks">How many consecutive runs each held-back finding has failed.</param>
/// <param name="ReportedFailedJobs">The watchdog Jobs whose failure has already been reported.</param>
/// <param name="ActionsAudit">State owned by the GitHub Actions schedule audit, kept verbatim as JSON.</param>
public sealed record AlertStateSnapshot(
    ImmutableDictionary<string, DateTimeOffset> LastAlerted,
    ImmutableDictionary<string, DateTimeOffset> FirstSeen,
    DateTimeOffset? LastCheck,
    ImmutableDictionary<string, int> SummaryStreaks,
    ImmutableList<string> ReportedFailedJobs,
    string? ActionsAudit)
{
    /// <summary>
    /// The key the run-failure cooldown is kept under, so a crashing watchdog does not alert every run.
    /// </summary>
    public const string RunFailedKey = "watchdog:run-failed";

    /// <summary>
    /// Gets the state of a watchdog that has never run.
    /// </summary>
    public static AlertStateSnapshot Empty { get; } = new(
        ImmutableDictionary<string, DateTimeOffset>.Empty,
        ImmutableDictionary<string, DateTimeOffset>.Empty,
        null,
        ImmutableDictionary<string, int>.Empty,
        [],
        null);
}
