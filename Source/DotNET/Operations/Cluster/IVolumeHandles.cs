// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// Reads which cloud volumes the cluster's PersistentVolumes refer to.
/// </summary>
public interface IVolumeHandles
{
    /// <summary>
    /// Reads the CSI volume handles of every PersistentVolume of a driver.
    /// </summary>
    /// <param name="driver">The CSI driver name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The volume handles.</returns>
    Task<IReadOnlySet<string>> Read(string driver, CancellationToken cancellationToken);
}
