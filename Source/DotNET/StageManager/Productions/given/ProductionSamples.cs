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
    /// The name of the sample Production.
    /// </summary>
    public const string Name = "ledger";

    /// <summary>
    /// The namespace of the sample Production.
    /// </summary>
    public const string Namespace = "apps";

    /// <summary>
    /// Gets the names of the sample Production.
    /// </summary>
    public static ProductionNames Names => new(Name, Namespace);

    /// <summary>
    /// A Production resource with the given spec.
    /// </summary>
    /// <param name="spec">The spec.</param>
    /// <returns>The Production.</returns>
    public static Production Production(ProductionSpec spec) => new()
    {
        Metadata = new() { Name = Name, NamespaceProperty = Namespace, Uid = "a6f1c5f2-6b0e-4c39-9e43-2f5f0c4a1b7d", Generation = 1 },
        Spec = spec
    };

    /// <summary>
    /// A Production on an external MongoDB, without a graph or named sinks.
    /// </summary>
    /// <returns>The spec.</returns>
    public static ProductionSpec External() => new()
    {
        Version = "19.148.0",
        Database = new() { Kind = StorageKinds.MongoDB, Mode = ProductionModes.External, ConnectionSecretName = "ledger-mongo" }
    };

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
    /// A Production on an external MongoDB with a managed graph, storing read models in PostgreSQL and SQL Server and graphs in two Neo4j servers.
    /// </summary>
    /// <returns>The spec.</returns>
    public static ProductionSpec WithSinks() => new()
    {
        Version = "15.0.0",
        Database = new() { Kind = StorageKinds.MongoDB, Mode = "External", ConnectionSecretName = "ledger-mongo" },
        Graph = new() { Mode = ProductionModes.Managed },
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
