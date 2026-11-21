// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Productions;

/// <summary>
/// The edition of an external Neo4j server.
/// </summary>
public enum GraphEdition
{
    /// <summary>
    /// One user database, shared by every graph.
    /// </summary>
    Community = 0,

    /// <summary>
    /// A database per event store and namespace.
    /// </summary>
    Enterprise = 1
}
