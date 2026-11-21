// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// What the watchdog reads of a node.
/// </summary>
/// <param name="Name">The node name.</param>
/// <param name="Conditions">Its conditions.</param>
public sealed record NodeInfo(string Name, IReadOnlyList<NodeCondition> Conditions);
