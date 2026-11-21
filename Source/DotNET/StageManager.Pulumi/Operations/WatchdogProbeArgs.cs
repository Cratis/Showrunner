// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Operations;

/// <summary>
/// The synthetic probe of the public endpoints, as a CronJob of its own inside the cluster. Its findings go through the same alert
/// pipeline and channels as every other check. A probe inside the cluster cannot see an ingress or load balancer that is down, so a
/// deployment that needs that runs <c>watchdog probe-external</c> from CI as well; that is not something Pulumi declares.
/// </summary>
public sealed class WatchdogProbeArgs
{
    /// <summary>
    /// Gets the public sites to call.
    /// </summary>
    public required IList<WatchdogProbeTargetArgs> Targets { get; init; }

    /// <summary>
    /// Gets the pause, in seconds, before the failing requests are made again; at most 30. Null is 30.
    /// </summary>
    public int? RoundGapSeconds { get; init; }

    /// <summary>
    /// Gets the schedule of the job, or null for every five minutes.
    /// </summary>
    public string? Schedule { get; init; }
}
