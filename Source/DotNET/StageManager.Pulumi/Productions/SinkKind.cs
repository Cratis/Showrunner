// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Productions;

/// <summary>
/// The storage engine of a named sink.
/// </summary>
public enum SinkKind
{
    /// <summary>
    /// MongoDB, for read models.
    /// </summary>
    MongoDB = 0,

    /// <summary>
    /// PostgreSQL, for read models.
    /// </summary>
    PostgreSql = 1,

    /// <summary>
    /// SQL Server, for read models.
    /// </summary>
    SqlServer = 2,

    /// <summary>
    /// Neo4j, for graphs.
    /// </summary>
    Neo4j = 3
}
