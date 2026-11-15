// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.Discovery;

namespace Cratis.Showrunner.StageManager.Understudies.given;

/// <summary>
/// A Chronicle that holds the event stores and namespaces it is given.
/// </summary>
/// <param name="topology">The topology.</param>
public class FakeChronicleDirectory(ChronicleTopology topology) : IChronicleDirectory
{
    public Task<ChronicleTopology> GetTopologyAsync(CancellationToken cancellationToken) => Task.FromResult(topology);
}
#endif
