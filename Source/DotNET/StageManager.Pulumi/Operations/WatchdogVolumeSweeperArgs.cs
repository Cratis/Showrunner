// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;

namespace Cratis.Showrunner.StageManager.Pulumi.Operations;

/// <summary>
/// The cloud account the <see cref="Watchdog"/> sweeps for orphaned CSI volumes, as a CronJob of its own: a volume that no
/// PersistentVolume refers to any more is detached and deleted, behind several fences. It deletes by default; set
/// <see cref="DryRun"/> to only report.
/// </summary>
public sealed class WatchdogVolumeSweeperArgs
{
    /// <summary>
    /// Gets the UpCloud API user. It is written to a Secret as a Pulumi secret.
    /// </summary>
    public required Input<string> Username { get; init; }

    /// <summary>
    /// Gets the UpCloud API password. It is written to a Secret as a Pulumi secret.
    /// </summary>
    public required Input<string> Password { get; init; }

    /// <summary>
    /// Gets a value indicating whether the sweep only reports what it would delete.
    /// </summary>
    public bool DryRun { get; init; }

    /// <summary>
    /// Gets how old an unused volume must be, in seconds, before it counts as an orphan. Null is 7200.
    /// </summary>
    public int? MinimumAgeSeconds { get; init; }

    /// <summary>
    /// Gets the most orphans one run acts on; more trips the fuse and nothing is deleted. Null is 10.
    /// </summary>
    public int? MaximumPerRun { get; init; }

    /// <summary>
    /// Gets the schedule of the job, or null for every hour.
    /// </summary>
    public string? Schedule { get; init; }
}
