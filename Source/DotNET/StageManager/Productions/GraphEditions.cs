// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// The Neo4j editions the graph of a <see cref="Production"/> can be.
/// </summary>
public static class GraphEditions
{
    /// <summary>
    /// Neo4j Community: one user database, so the kernel stores every graph in it.
    /// </summary>
    public const string Community = "Community";

    /// <summary>
    /// Neo4j Enterprise: a database per event store and namespace, under a prefix.
    /// </summary>
    public const string Enterprise = "Enterprise";

    /// <summary>
    /// Gets every edition.
    /// </summary>
    public static IReadOnlyList<string> All { get; } = [Community, Enterprise];
}
