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
