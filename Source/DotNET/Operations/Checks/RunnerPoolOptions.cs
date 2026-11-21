// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Checks;

/// <summary>
/// The options of <see cref="RunnerPoolCheck"/>.
/// </summary>
public sealed record RunnerPoolOptions
{
    /// <summary>
    /// Gets the namespace the runner scale sets run in.
    /// </summary>
    public required string Namespace { get; init; }

    /// <summary>
    /// Gets the scale sets that are watched; runners of other scale sets are left alone.
    /// </summary>
    public required IReadOnlyList<string> ScaleSets { get; init; }

    /// <summary>
    /// Gets how long a job may hold a runner before it is stopped, or <see cref="TimeSpan.Zero"/> to only report.
    /// </summary>
    public TimeSpan MaximumJobTime { get; init; } = TimeSpan.FromMinutes(90);

    /// <summary>
    /// Gets how long a runner without a job may stay pending before it is replaced.
    /// </summary>
    public TimeSpan PendingTime { get; init; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Gets the name of the ConfigMap that remembers when each runner was first seen holding a job.
    /// </summary>
    public string StateConfigMap { get; init; } = "runner-watchdog-state";

    /// <summary>
    /// Gets the most runners one pass may stop, and the most it may replace. More than this over the limit at once is far
    /// likelier to be a wrong clock, a lost state record or a GitHub outage than that many hung jobs, so nothing is stopped.
    /// </summary>
    public int Cap { get; init; } = 3;
}
