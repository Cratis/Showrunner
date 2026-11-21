// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Operations;

/// <summary>
/// A GitHub Actions runner pool the <see cref="Watchdog"/> keeps from filling up with hung work, as a CronJob of its own: a job
/// that has held a runner for too long is stopped by deleting its ephemeral runner, and a runner stuck pending is replaced.
/// </summary>
public sealed class WatchdogRunnersArgs
{
    /// <summary>
    /// Gets the namespace the runner scale sets run in.
    /// </summary>
    public required string Namespace { get; init; }

    /// <summary>
    /// Gets the scale sets to watch; runners of other scale sets are left alone.
    /// </summary>
    public required IList<string> ScaleSets { get; init; }

    /// <summary>
    /// Gets how long a job may hold a runner, in minutes, before it is stopped; 0 only reports. Null is 90.
    /// </summary>
    public int? MaximumJobMinutes { get; init; }

    /// <summary>
    /// Gets how long a runner without a job may stay pending, in minutes, before it is replaced. Null is 30.
    /// </summary>
    public int? PendingMinutes { get; init; }

    /// <summary>
    /// Gets the schedule of the job, or null for every five minutes.
    /// </summary>
    public string? Schedule { get; init; }
}
