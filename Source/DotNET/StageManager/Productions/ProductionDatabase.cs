// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// How the database behind a <see cref="Production"/> is provided.
/// </summary>
public class ProductionDatabase
{
    /// <summary>
    /// The Percona Server for MongoDB image a managed database runs when none is given.
    /// </summary>
    public const string DefaultImage = "percona/percona-server-mongodb:7.0.14-8";

    /// <summary>
    /// The bytes per second at which MongoDB returns freed allocator memory to the operating system, when none is given.
    /// </summary>
    /// <remarks>
    /// MongoDB 8.0 never returns freed tcmalloc pages unless this is set (default 0). Measured in production, members held 2.4-3.3 GiB of free pages on top of a 2-3 GiB cache.
    /// </remarks>
    public const long DefaultTcmallocReleaseRate = 16_777_216;

    /// <summary>
    /// Gets or sets the storage engine: <c>MongoDB</c>, <c>PostgreSql</c> or <c>SqlServer</c>. The SQL engines are External only.
    /// </summary>
    [Description("MongoDB (the default), PostgreSql or SqlServer. PostgreSql and SqlServer are External only. Cannot be changed once the Production is running.")]
    public string Kind { get; set; } = StorageKinds.MongoDB;

    /// <summary>
    /// Gets or sets the mode: <c>Managed</c> or <c>External</c>.
    /// </summary>
    [Description("Managed or External. Cannot be changed once the Production is running.")]
    public string Mode { get; set; } = ProductionModes.External;

    /// <summary>
    /// Gets or sets the name of the Secret holding the connection string of an external database.
    /// </summary>
    [Description("The name of the Secret holding the connection string of an external database, under the key connectionString. Mounted as a file, never copied.")]
    public string ConnectionSecretName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the database holding the System event store.
    /// </summary>
    [Description("The name of the database holding Chronicle's System event store. Empty uses the kernel's default, _system. Set System for an installation that predates Chronicle 19.117, whose data is in a database of that name.")]
    public string SystemDatabaseName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of members of a managed replica set.
    /// </summary>
    [Description("For a Managed database: the members of its replica set, 1 for development or 3 or 5 for production.")]
    public int Members { get; set; } = 1;

    /// <summary>
    /// Gets or sets whether the members of a managed database must run on different nodes.
    /// </summary>
    [Description("For a Managed database: run one member per node. Defaults to true; set false on a single-node cluster such as kind.")]
    public bool OneMemberPerNode { get; set; } = true;

    /// <summary>
    /// Gets or sets the memory limit of each member of a managed database.
    /// </summary>
    [Description("For a Managed database: the memory limit of each member. Defaults to 8Gi.")]
    public string MemoryLimit { get; set; } = "8Gi";

    /// <summary>
    /// Gets or sets the memory each member requests.
    /// </summary>
    [Description("For a Managed database: the memory each member requests. Empty requests nothing, so the limit alone sizes the pod.")]
    public string MemoryRequest { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the CPU each member requests.
    /// </summary>
    [Description("For a Managed database: the CPU each member requests, such as 500m. Empty requests nothing.")]
    public string CpuRequest { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the CPU limit of each member.
    /// </summary>
    [Description("For a Managed database: the CPU limit of each member, such as 2. Empty sets no CPU limit.")]
    public string CpuLimit { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the share of the cache budget WiredTiger uses.
    /// </summary>
    [Description("For a Managed database: the share of the cache budget WiredTiger uses, 0 to 1 exclusive. Percona sizes the cache as this ratio times the memory limit minus 1 GB, so the default of 0.5 gives a small cache at a small limit. 0 keeps the operator's default.")]
    public double WiredTigerCacheSizeRatio { get; set; }

    /// <summary>
    /// Gets or sets the bytes per second at which freed allocator memory goes back to the operating system.
    /// </summary>
    [Description("For a Managed database: the bytes per second at which MongoDB hands freed allocator memory back to the operating system (tcmallocReleaseRate). Defaults to 16777216; 0 leaves MongoDB's own default, which never releases it.")]
    public long TcmallocReleaseRate { get; set; } = DefaultTcmallocReleaseRate;

    /// <summary>
    /// Gets or sets further <c>mongod</c> <c>setParameter</c> values.
    /// </summary>
    [Description("For a Managed database: further mongod setParameter values, by name. Numbers and booleans are written as such; anything else as a string.")]
    public IDictionary<string, string> SetParameters { get; set; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets the MongoDB metrics for Prometheus.
    /// </summary>
    [Description("For a Managed database: the MongoDB metrics for Prometheus.")]
    public ProductionDatabaseMetrics Metrics { get; set; } = new();

    /// <summary>
    /// Gets or sets the size of the volume of each member of a managed database.
    /// </summary>
    [Description("For a Managed database: the size of the volume of each member. Defaults to 10Gi.")]
    public string StorageSize { get; set; } = "10Gi";

    /// <summary>
    /// Gets or sets the storage class of the volumes of a managed database.
    /// </summary>
    [Description("For a Managed database: the storage class of its volumes. Empty uses the cluster's default.")]
    public string StorageClassName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the nodes the members of a managed database may run on.
    /// </summary>
    [Description("For a Managed database: the node labels its members must match.")]
    public IDictionary<string, string> NodeSelector { get; set; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets the image of a managed database.
    /// </summary>
    [Description("For a Managed database: the Percona Server for MongoDB image. Defaults to a pinned version; it is never upgraded automatically.")]
    public string Image { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the backup agent of a managed database.
    /// </summary>
    [Description("For a Managed database: the backup agent Percona runs next to every member. This is where an Understudy attaches the storage it backs up to.")]
    public ProductionDatabaseBackup Backup { get; set; } = new();

    /// <summary>
    /// Gets whether the database is external.
    /// </summary>
    /// <returns>True when the mode is External.</returns>
    public bool IsExternal() => string.Equals(Mode, ProductionModes.External, StringComparison.Ordinal);

    /// <summary>
    /// Gets the image to run, with the default applied.
    /// </summary>
    /// <returns>The image.</returns>
    public string ImageOrDefault() => string.IsNullOrWhiteSpace(Image) ? DefaultImage : Image;
}
