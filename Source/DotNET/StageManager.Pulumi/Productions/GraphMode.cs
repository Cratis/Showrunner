// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Productions;

/// <summary>
/// Whether a Production has a Neo4j graph, and who runs it.
/// </summary>
public enum GraphMode
{
    /// <summary>
    /// No graph.
    /// </summary>
    None = 0,

    /// <summary>
    /// Stage Manager runs a Neo4j Community server.
    /// </summary>
    Managed = 1,

    /// <summary>
    /// A Neo4j server that something else runs.
    /// </summary>
    External = 2
}
