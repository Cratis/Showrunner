// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;

namespace Cratis.Showrunner.StageManager.Pulumi.Productions;

/// <summary>
/// The database of a Production: one that something else runs and the Production connects to, or one that Stage Manager runs.
/// </summary>
public sealed class ProductionDatabaseArgs
{
    /// <summary>
    /// Gets the storage engine. Defaults to MongoDB.
    /// </summary>
    public DatabaseKind Kind { get; init; } = DatabaseKind.MongoDB;

    /// <summary>
    /// Gets who runs the database. Defaults to <see cref="DatabaseMode.External"/>. It cannot be changed once the Production is running.
    /// </summary>
    public DatabaseMode Mode { get; init; } = DatabaseMode.External;

    /// <summary>
    /// Gets the name of the Secret holding the connection string of an external database, under the key <c>connectionString</c>.
    /// It is created, under this name, when <see cref="ConnectionString"/> is given; otherwise it must already exist.
    /// Defaults to <c>&lt;name&gt;-database</c>.
    /// </summary>
    public string? ConnectionSecretName { get; init; }

    /// <summary>
    /// Gets the connection string of an external database, which becomes a Pulumi secret in a Kubernetes Secret.
    /// </summary>
    public Input<string>? ConnectionString { get; init; }

    /// <summary>
    /// Gets the name of the database holding the System event store, or null for the kernel's default (<c>_system</c>).
    /// Set <c>System</c> for an installation that predates Chronicle 19.117.
    /// </summary>
    public string? SystemDatabaseName { get; init; }

    /// <summary>
    /// Gets the members of the replica set of a managed database: 1 for development, 3 or 5 for production.
    /// </summary>
    public int? Members { get; init; }

    /// <summary>
    /// Gets a value indicating whether the members of a managed database run on different nodes, or null for the operator's default (true).
    /// Set false on a single-node cluster.
    /// </summary>
    public bool? OneMemberPerNode { get; init; }

    /// <summary>
    /// Gets the memory limit of each member of a managed database, such as <c>8Gi</c>.
    /// </summary>
    public string? MemoryLimit { get; init; }

    /// <summary>
    /// Gets the memory each member of a managed database requests, such as <c>1Gi</c>, or null to request none.
    /// </summary>
    public string? MemoryRequest { get; init; }

    /// <summary>
    /// Gets the CPU each member of a managed database requests, such as <c>500m</c>, or null to request none.
    /// </summary>
    public string? CpuRequest { get; init; }

    /// <summary>
    /// Gets the CPU limit of each member of a managed database, such as <c>2</c>, or null for none.
    /// </summary>
    public string? CpuLimit { get; init; }

    /// <summary>
    /// Gets the share of the cache budget WiredTiger uses, above 0 and below 1, or null for the operator's default.
    /// Percona sizes the cache as this ratio times the memory limit minus 1 GB.
    /// </summary>
    public double? WiredTigerCacheSizeRatio { get; init; }

    /// <summary>
    /// Gets the bytes per second at which MongoDB hands freed allocator memory back to the operating system, or null for the operator's default (16777216). 0 leaves MongoDB's own default, which never releases it.
    /// </summary>
    public int? TcmallocReleaseRate { get; init; }

    /// <summary>
    /// Gets further <c>mongod</c> <c>setParameter</c> values of a managed database, by name.
    /// </summary>
    public IDictionary<string, string> SetParameters { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Gets the MongoDB metrics of a managed database: one exporter per member for Prometheus.
    /// </summary>
    public DatabaseMetricsArgs? Metrics { get; init; }

    /// <summary>
    /// Gets the size of the volume of each member of a managed database, such as <c>20Gi</c>.
    /// </summary>
    public string? StorageSize { get; init; }

    /// <summary>
    /// Gets the storage class of the volumes of a managed database, or null for the cluster's default.
    /// </summary>
    public string? StorageClassName { get; init; }

    /// <summary>
    /// Gets the node labels the members of a managed database must match.
    /// </summary>
    public IDictionary<string, string> NodeSelector { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Gets the Percona Server for MongoDB image of a managed database, or null for the operator's pinned version.
    /// </summary>
    public string? Image { get; init; }

    /// <summary>
    /// Gets the backup agent of a managed database.
    /// </summary>
    public DatabaseBackupArgs? Backup { get; init; }

    /// <summary>
    /// Builds the spec of the database.
    /// </summary>
    /// <param name="connectionSecretName">The name of the Secret holding the connection string, if any.</param>
    /// <param name="backupCredentialsSecretName">The name of the Secret holding the backup storage credentials, if any.</param>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec(string? connectionSecretName, string? backupCredentialsSecretName) =>
        SpecExtensions.NewSpec()
            .PutEnum("kind", Kind)
            .PutEnum("mode", Mode)
            .Put("connectionSecretName", connectionSecretName)
            .Put("systemDatabaseName", SystemDatabaseName)
            .Put("members", Members)
            .Put("oneMemberPerNode", OneMemberPerNode)
            .Put("memoryLimit", MemoryLimit)
            .Put("memoryRequest", MemoryRequest)
            .Put("cpuRequest", CpuRequest)
            .Put("cpuLimit", CpuLimit)
            .Put("wiredTigerCacheSizeRatio", WiredTigerCacheSizeRatio)
            .Put("tcmallocReleaseRate", TcmallocReleaseRate)
            .PutMap("setParameters", SetParameters.Select(pair => new KeyValuePair<string, object>(pair.Key, pair.Value)))
            .Put("storageSize", StorageSize)
            .Put("storageClassName", StorageClassName)
            .PutMap("nodeSelector", NodeSelector.Select(pair => new KeyValuePair<string, object>(pair.Key, pair.Value)))
            .Put("image", Image)
            .Put("metrics", Metrics?.ToSpec())
            .Put("backup", Backup?.ToSpec(backupCredentialsSecretName));
}
