// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Checks;

/// <summary>
/// The options of <see cref="VolumeSweepCheck"/>.
/// </summary>
public sealed record VolumeSweepOptions
{
    /// <summary>
    /// Gets the CSI driver whose volumes are swept.
    /// </summary>
    public string CsiDriver { get; init; } = "storage.csi.upcloud.com";

    /// <summary>
    /// Gets the label the CSI driver puts on the volumes it provisions for a cluster.
    /// </summary>
    public string ClusterLabel { get; init; } = "csi_cluster_id";

    /// <summary>
    /// Gets how old an unused volume must be before it counts as an orphan, so one that is still being provisioned is left alone.
    /// </summary>
    public TimeSpan MinimumAge { get; init; } = TimeSpan.FromSeconds(7200);

    /// <summary>
    /// Gets the most orphans one run acts on; more than that trips the fuse and nothing is deleted.
    /// </summary>
    public int MaximumPerRun { get; init; } = 10;

    /// <summary>
    /// Gets a value indicating whether the sweep only reports what it would delete.
    /// </summary>
    public bool DryRun { get; init; }

    /// <summary>
    /// Gets how long to wait between attempts to delete a volume that is still detaching.
    /// </summary>
    public TimeSpan DetachPoll { get; init; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets how many times to try to delete a volume that is still detaching.
    /// </summary>
    public int DetachAttempts { get; init; } = 12;
}
