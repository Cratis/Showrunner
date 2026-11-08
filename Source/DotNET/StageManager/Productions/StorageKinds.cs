// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// The storage engines a <see cref="Production"/> names, for its database and for its sinks.
/// </summary>
public static class StorageKinds
{
    /// <summary>
    /// MongoDB, the default for the database.
    /// </summary>
    public const string MongoDB = "MongoDB";

    /// <summary>
    /// PostgreSQL.
    /// </summary>
    public const string PostgreSql = "PostgreSql";

    /// <summary>
    /// Microsoft SQL Server.
    /// </summary>
    public const string SqlServer = "SqlServer";

    /// <summary>
    /// Neo4j, for graphs only.
    /// </summary>
    public const string Neo4j = "Neo4j";

    /// <summary>
    /// Gets the kinds the database of a Production can be.
    /// </summary>
    public static IReadOnlyList<string> Database { get; } = [MongoDB, PostgreSql, SqlServer];

    /// <summary>
    /// Gets the kinds a named sink can be.
    /// </summary>
    public static IReadOnlyList<string> Sinks { get; } = [MongoDB, PostgreSql, SqlServer, Neo4j];

    /// <summary>
    /// Gets whether a kind is a SQL engine.
    /// </summary>
    /// <param name="kind">The kind.</param>
    /// <returns>True for PostgreSQL and SQL Server.</returns>
    public static bool IsSql(string kind) => string.Equals(kind, PostgreSql, StringComparison.Ordinal) || string.Equals(kind, SqlServer, StringComparison.Ordinal);
}
