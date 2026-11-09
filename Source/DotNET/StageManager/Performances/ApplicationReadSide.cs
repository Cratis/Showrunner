// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageManager.Productions;

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// The read side of the named sinks and the graph a Production gives an application: what it needs to read its read models and graphs where the kernel writes them.
/// </summary>
/// <param name="MongoDBSinks">The named MongoDB sinks the application uses, each mounted as a file Cratis Arc reads through <c>Cratis:MongoDB:Sinks:&lt;name&gt;:ServerFile</c>.</param>
/// <param name="Graph">The graph of the Production bound to Cratis.Arc.Neo4j, or null when the application does not read it.</param>
/// <param name="Unreadable">The sinks the application uses that Cratis Arc cannot read: SQL sinks, and Neo4j sinks on a server other than the graph of the Production.</param>
/// <param name="GraphWanted">Whether the application reads the graph of the Production, whether or not the Production has one.</param>
/// <remarks>
/// Every connection is a file mounted from its Secret or a Kubernetes <c>secretKeyRef</c>; no value is ever written into the spec of a workload.
/// Reading named MongoDB sinks needs the Cratis Arc release with Cratis/Arc#3770.
/// </remarks>
public record ApplicationReadSide(IReadOnlyList<MongoDBSinkBinding> MongoDBSinks, GraphBinding? Graph, IReadOnlyList<string> Unreadable, bool GraphWanted)
{
    /// <summary>
    /// The directory the connections of the named sinks are mounted in, one file per sink, named after the sink.
    /// </summary>
    public const string SinksDirectory = "/run/secrets/sinks";

    /// <summary>
    /// The name of the volume the connections of the named sinks are mounted from.
    /// </summary>
    public const string SinksVolume = "sinks";

    const string Neo4j = "Cratis__Neo4j__";

    /// <summary>
    /// Gets a read side that binds nothing.
    /// </summary>
    public static ApplicationReadSide None { get; } = new([], null, [], false);

    /// <summary>
    /// Gets the Secret keys the application workload needs, which must exist in the namespace of the Performance.
    /// </summary>
    public IEnumerable<ResolvedSecretRef> Secrets =>
        MongoDBSinks.Select(sink => sink.Connection).Concat(Graph?.Secrets ?? []);

    /// <summary>
    /// Gets the environment variables that hold no secret: the path of each sink's connection file and the plain settings of the graph.
    /// </summary>
    public IReadOnlyDictionary<string, string> EnvironmentVariables
    {
        get
        {
            var variables = MongoDBSinks.ToDictionary(sink => sink.Setting, sink => sink.File, StringComparer.Ordinal);
            if (Graph is { } graph)
            {
                if (graph.Server is { } server)
                {
                    variables[$"{Neo4j}Server"] = server;
                }

                variables[$"{Neo4j}UserName"] = graph.UserName;
                variables[$"{Neo4j}DatabaseMode"] = graph.DatabaseMode;
                if (graph.DatabasePrefix is { } prefix)
                {
                    variables[$"{Neo4j}DatabasePrefix"] = prefix;
                }

                if (graph.EventStore is { } eventStore)
                {
                    variables[$"{Neo4j}EventStore"] = eventStore;
                }
            }

            return variables;
        }
    }

    /// <summary>
    /// Gets the environment variables Kubernetes fills from a Secret key: the graph's password, and its bolt URI when it is only in a Secret.
    /// </summary>
    public IReadOnlyDictionary<string, ResolvedSecretRef> SecretEnvironmentVariables
    {
        get
        {
            var variables = new Dictionary<string, ResolvedSecretRef>(StringComparer.Ordinal);
            if (Graph is { } graph)
            {
                variables[$"{Neo4j}Password"] = graph.Password;
                if (graph.ServerSecret is { } server)
                {
                    variables[$"{Neo4j}Server"] = server;
                }
            }

            return variables;
        }
    }

    /// <summary>
    /// Gets the folder the connections of the named MongoDB sinks are mounted in, or null when the application uses none.
    /// </summary>
    public SecretMount? Mount => MongoDBSinks.Count == 0
        ? null
        : new(SinksVolume, SinksDirectory, [.. MongoDBSinks.Select(sink => new SecretSource(sink.Connection.SecretName, new Dictionary<string, string> { [sink.Connection.Key] = sink.Name }))]);

    /// <summary>
    /// Works out the read side of an application on a Production. The Production must configure every sink the application uses.
    /// </summary>
    /// <param name="application">The application.</param>
    /// <param name="used">The names of the sinks the application uses, from <see cref="ApplicationSinks.UsedBy"/>.</param>
    /// <param name="production">The spec of the Production.</param>
    /// <param name="names">The names of the Production, for the Secret of its managed graph.</param>
    /// <returns>The read side.</returns>
    public static ApplicationReadSide For(ApplicationSpec application, IEnumerable<string> used, ProductionSpec production, ProductionNames names)
    {
        var mongoDB = new List<MongoDBSinkBinding>();
        var unreadable = new List<string>();
        var graphWanted = application.Graph;

        foreach (var name in used)
        {
            var sink = production.Sinks.Configurations.FirstOrDefault(configuration => string.Equals(configuration.Name, name, StringComparison.OrdinalIgnoreCase));
            switch (sink)
            {
                case null:
                    break;
                case { Kind: StorageKinds.MongoDB, ConnectionSecretRef: { } reference }:
                    mongoDB.Add(new(sink.Name, new(reference.Name, reference.KeyOrDefault())));
                    break;
                case { Kind: StorageKinds.Neo4j } when !string.IsNullOrWhiteSpace(sink.GraphRef):
                    graphWanted = true;
                    break;
                default:
                    unreadable.Add(sink.Name);
                    break;
            }
        }

        GraphBinding? graph = null;
        if (graphWanted && ChronicleKernelConfiguration.GraphOf(production, names) is { } productionGraph)
        {
            var managed = production.Graph.Mode == ProductionModes.Managed;
            graph = new(
                managed ? names.Neo4jServer : null,
                managed ? null : new(productionGraph.SecretName, ProductionGraph.ServerKey),
                productionGraph.UserName,
                new(productionGraph.SecretName, ProductionGraph.PasswordKey),
                productionGraph.DatabaseMode,
                productionGraph.DatabasePrefix,
                application.EventStores.FirstOrDefault(),
                production.Graph.Edition == GraphEditions.Community || managed);
        }

        return new(mongoDB, graph, unreadable, graphWanted);
    }
}
