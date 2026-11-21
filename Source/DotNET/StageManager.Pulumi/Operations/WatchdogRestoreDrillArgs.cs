// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Operations;

/// <summary>
/// A restore drill CronJob that the <see cref="Watchdog"/> expects to keep succeeding.
/// </summary>
public sealed class WatchdogRestoreDrillArgs
{
    /// <summary>
    /// Gets the namespace the drill runs in.
    /// </summary>
    public required string Namespace { get; init; }

    /// <summary>
    /// Gets the name of the drill CronJob, or null for <c>restore-drill</c>.
    /// </summary>
    public string? CronJob { get; init; }

    /// <summary>
    /// Gets how long the drill may go without succeeding, or null for <c>30h</c>.
    /// </summary>
    public string? StaleAfter { get; init; }
}
