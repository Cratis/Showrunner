// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies.Discovery;

/// <summary>
/// Asks a Production's Chronicle which event stores and namespaces it holds.
/// </summary>
public interface IChronicleDirectory
{
    /// <summary>
    /// Reads the topology.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The topology.</returns>
    Task<ChronicleTopology> GetTopologyAsync(CancellationToken cancellationToken);
}
