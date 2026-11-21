// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Productions;

/// <summary>
/// The Neo4j graph of a Production, where graph projections land.
/// </summary>
public sealed class ProductionGraphArgs
{
    /// <summary>
    /// Gets whether there is a graph and who runs it. Defaults to <see cref="GraphMode.Managed"/>: naming a graph means wanting one.
    /// It cannot be changed once the Production is running.
    /// </summary>
    public GraphMode Mode { get; init; } = GraphMode.Managed;

    /// <summary>
    /// Gets the edition, or null for the operator's default (<see cref="GraphEdition.Community"/>).
    /// </summary>
    public GraphEdition? Edition { get; init; }

    /// <summary>
    /// Gets the name of the existing Secret of an external graph, holding its bolt URI under the key <c>server</c> and its
    /// password under the key <c>password</c>.
    /// </summary>
    public string? CredentialsSecretName { get; init; }

    /// <summary>
    /// Gets the user of an external graph, or null for the operator's default (<c>neo4j</c>).
    /// </summary>
    public string? UserName { get; init; }

    /// <summary>
    /// Gets the prefix of the database names on an Enterprise server, so several Productions can share one server.
    /// </summary>
    public string? DatabasePrefix { get; init; }

    /// <summary>
    /// Gets the image of a managed graph, or null for the operator's pinned version.
    /// </summary>
    public string? Image { get; init; }

    /// <summary>
    /// Gets the size of the volume of a managed graph, such as <c>10Gi</c>.
    /// </summary>
    public string? StorageSize { get; init; }

    /// <summary>
    /// Gets the storage class of the volume of a managed graph, or null for the cluster's default.
    /// </summary>
    public string? StorageClassName { get; init; }

    /// <summary>
    /// Gets the node labels the pod of a managed graph must match.
    /// </summary>
    public IDictionary<string, string> NodeSelector { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Builds the spec of the graph.
    /// </summary>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec() =>
        SpecExtensions.NewSpec()
            .PutEnum("mode", Mode)
            .PutEnum("edition", Edition)
            .Put("credentialsSecretName", CredentialsSecretName)
            .Put("userName", UserName)
            .Put("databasePrefix", DatabasePrefix)
            .Put("image", Image)
            .Put("storageSize", StorageSize)
            .Put("storageClassName", StorageClassName)
            .PutMap("nodeSelector", NodeSelector.Select(pair => new KeyValuePair<string, object>(pair.Key, pair.Value)));
}
