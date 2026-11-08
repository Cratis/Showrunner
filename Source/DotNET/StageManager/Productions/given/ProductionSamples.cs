// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.StageManager.Productions.given;

/// <summary>
/// Productions the specifications start from.
/// </summary>
public static class ProductionSamples
{
    /// <summary>
    /// A Production on a managed MongoDB, without named sinks.
    /// </summary>
    /// <returns>The spec.</returns>
    public static ProductionSpec Managed() => new()
    {
        Version = "15.0.0",
        Database = new() { Kind = StorageKinds.MongoDB, Mode = "Managed" }
    };

    /// <summary>
    /// A Production on an external MongoDB, storing read models in PostgreSQL and SQL Server and graphs in two Neo4j servers.
    /// </summary>
    /// <returns>The spec.</returns>
    public static ProductionSpec WithSinks() => new()
    {
        Version = "15.0.0",
        Database = new() { Kind = StorageKinds.MongoDB, Mode = "External", ConnectionSecretName = "ledger-mongo" },
        Sinks = new()
        {
            ReadModels = "reporting",
            Graphs = "people",
            Configurations =
            [
                new() { Name = "reporting", Kind = StorageKinds.PostgreSql, ConnectionSecretRef = new() { Name = "reporting-pg" } },
                new() { Name = "warehouse", Kind = StorageKinds.SqlServer, ConnectionSecretRef = new() { Name = "warehouse-sql", Key = "dsn" } },
                new() { Name = "documents", Kind = StorageKinds.MongoDB, ConnectionSecretRef = new() { Name = "documents-mongo" } },
                new() { Name = "people", Kind = StorageKinds.Neo4j, GraphRef = "managed" },
                new() { Name = "organization", Kind = StorageKinds.Neo4j, ConnectionSecretRef = new() { Name = "organization-graph", Key = "server" } }
            ]
        }
    };
}
#endif
