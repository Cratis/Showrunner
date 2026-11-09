// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// The Neo4j graph of a <see cref="Production"/>, where graph projections land.
/// </summary>
public class ProductionGraph
{
    /// <summary>
    /// The key of the credentials Secret that holds the bolt URI of the server.
    /// </summary>
    public const string ServerKey = "server";

    /// <summary>
    /// The key of the credentials Secret that holds the password.
    /// </summary>
    public const string PasswordKey = "password";

    /// <summary>
    /// The user a managed graph authenticates with.
    /// </summary>
    public const string ManagedUserName = "neo4j";

    /// <summary>
    /// The Neo4j image a managed graph runs when none is given.
    /// </summary>
    public const string DefaultImage = "neo4j:5.26.0-community";

    /// <summary>
    /// Gets or sets the mode: <c>None</c>, <c>Managed</c> or <c>External</c>.
    /// </summary>
    [Description("None (the default), Managed or External. Cannot be changed once the Production is running.")]
    public string Mode { get; set; } = ProductionModes.None;

    /// <summary>
    /// Gets or sets the edition: <c>Community</c> or <c>Enterprise</c>.
    /// </summary>
    [Description("Community (the default) or Enterprise. Community has one user database, so every graph shares it. Enterprise gives each event store and namespace its own database, under databasePrefix. A Managed graph is Community. Cannot be changed once the Production is running.")]
    public string Edition { get; set; } = GraphEditions.Community;

    /// <summary>
    /// Gets or sets the Secret of an external graph, holding its bolt URI and password.
    /// </summary>
    [Description("For an External graph: the Secret holding the bolt URI under the key server and the password under the key password. Mounted as files, never copied.")]
    public string CredentialsSecretName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user of an external graph.
    /// </summary>
    [Description("For an External graph: the user to authenticate with. Defaults to neo4j.")]
    public string UserName { get; set; } = ManagedUserName;

    /// <summary>
    /// Gets or sets the prefix of the database names on an Enterprise server.
    /// </summary>
    [Description("For an Enterprise graph: the prefix of the database names, so several Productions can share one server. Defaults to the name of the Production.")]
    public string DatabasePrefix { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the image of a managed graph.
    /// </summary>
    [Description("For a Managed graph: the Neo4j Community image. Defaults to a pinned version.")]
    public string Image { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the size of the volume of a managed graph.
    /// </summary>
    [Description("For a Managed graph: the size of its volume. Defaults to 10Gi.")]
    public string StorageSize { get; set; } = "10Gi";

    /// <summary>
    /// Gets or sets the storage class of the volume of a managed graph.
    /// </summary>
    [Description("For a Managed graph: the storage class of its volume. Empty uses the cluster's default.")]
    public string StorageClassName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the nodes a managed graph may run on.
    /// </summary>
    [Description("For a Managed graph: the node labels its pod must match.")]
    public IDictionary<string, string> NodeSelector { get; set; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Gets the image to run, with the default applied.
    /// </summary>
    /// <returns>The image.</returns>
    public string ImageOrDefault() => string.IsNullOrWhiteSpace(Image) ? DefaultImage : Image;
}
