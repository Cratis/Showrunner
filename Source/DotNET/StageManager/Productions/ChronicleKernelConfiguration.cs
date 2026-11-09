// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageManager.Performances;

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// The configuration of the Chronicle kernel of a <see cref="Production"/>: the settings that are plain values, and the connections, which are mounted from their Secrets as files and never become an environment value.
/// </summary>
/// <param name="EnvironmentVariables">The kernel settings that hold no secret, as environment variables, including the path of every connection file the kernel reads.</param>
/// <param name="Connections">The Secrets mounted into the kernel, each key as a file named after the kernel setting it holds.</param>
/// <remarks>
/// Every connection file is named after its kernel setting, such as <c>Cratis__Chronicle__Storage__ConnectionDetails</c>, in <see cref="ConnectionsDirectory"/>.
/// The kernel reads it through the matching <c>…File</c> setting, such as <c>Cratis__Chronicle__Storage__ConnectionDetailsFile</c>, which holds its path.
/// Only the <c>…File</c> form is set: the kernel refuses to start when a setting and its <c>…File</c> are both set.
/// A Neo4j server is read through <c>…ServerFile</c>, which needs the Chronicle release with Cratis/Chronicle#5771.
/// </remarks>
public record ChronicleKernelConfiguration(IReadOnlyDictionary<string, string> EnvironmentVariables, SecretMount Connections)
{
    /// <summary>
    /// The directory the connection files are mounted in.
    /// </summary>
    public const string ConnectionsDirectory = "/run/secrets/chronicle";

    /// <summary>
    /// The name of the volume the connection files are mounted from.
    /// </summary>
    public const string ConnectionsVolume = "chronicle-connections";

    const string Prefix = "Cratis__Chronicle__";
    const string StorageType = $"{Prefix}Storage__Type";
    const string StorageConnection = $"{Prefix}Storage__ConnectionDetails";
    const string SinksReadModels = $"{Prefix}Sinks__ReadModels";
    const string SinksGraphs = $"{Prefix}Sinks__Graphs";
    const string SinkConfigurations = $"{Prefix}Sinks__Configurations__";
    const string Neo4j = $"{Prefix}Neo4j__";
    const string FileSuffix = "File";

    /// <summary>
    /// Gets the kernel settings whose value is a mounted file, with the path of that file.
    /// </summary>
    public IReadOnlyDictionary<string, string> ConnectionFiles =>
        Connections.Sources
            .SelectMany(source => source.Items?.Values ?? [])
            .ToDictionary(setting => setting, FileOf, StringComparer.Ordinal);

    /// <summary>
    /// Builds the kernel configuration of a Production. The spec must be valid by <see cref="ProductionValidator"/>.
    /// </summary>
    /// <param name="spec">The spec of the Production.</param>
    /// <param name="names">The names of what the Production owns, for the Secrets of its managed database and graph.</param>
    /// <returns>The kernel configuration.</returns>
    public static ChronicleKernelConfiguration Build(ProductionSpec spec, ProductionNames names)
    {
        var variables = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [StorageType] = StorageTypeOf(spec.Database.Kind)
        };
        var sources = new List<SecretSource>();

        // A managed database gets a connection Secret of its own, composed by the operator from the users Percona keeps.
        var databaseSecret = spec.Database.IsExternal() ? spec.Database.ConnectionSecretName : names.MongoDBConnection;
        sources.Add(new(databaseSecret, new Dictionary<string, string> { [ProductionSecretReference.DefaultKey] = StorageConnection }));
        variables[$"{StorageConnection}{FileSuffix}"] = FileOf(StorageConnection);

        var graph = GraphOf(spec, names);
        if (graph is not null)
        {
            sources.Add(new(graph.SecretName, new Dictionary<string, string>
            {
                [ProductionGraph.ServerKey] = $"{Neo4j}Server",
                [ProductionGraph.PasswordKey] = $"{Neo4j}Password"
            }));
            SetGraph(variables, Neo4j, graph);
        }

        var sinks = spec.Sinks;
        if (!string.IsNullOrWhiteSpace(sinks.ReadModels))
        {
            variables[SinksReadModels] = sinks.ReadModels;
        }

        if (!string.IsNullOrWhiteSpace(sinks.Graphs))
        {
            variables[SinksGraphs] = sinks.Graphs;
        }

        foreach (var sink in sinks.Configurations)
        {
            var setting = $"{SinkConfigurations}{sink.Name}__";
            variables[$"{setting}Type"] = SinkTypeOf(sink.Kind);

            if (sink.ConnectionSecretRef is { } reference)
            {
                var connection = sink.Kind == StorageKinds.Neo4j ? $"{setting}Server" : $"{setting}ConnectionDetails";
                sources.Add(new(reference.Name, new Dictionary<string, string> { [reference.KeyOrDefault()] = connection }));
                variables[$"{connection}{FileSuffix}"] = FileOf(connection);
            }
            else if (sink.Kind == StorageKinds.Neo4j && graph is not null)
            {
                // A sink on the Production's own graph reads the files mounted for it.
                SetGraph(variables, setting, graph);
            }
        }

        return new(variables, new(ConnectionsVolume, ConnectionsDirectory, sources));
    }

    /// <summary>
    /// Gets the Chronicle storage type of a database kind.
    /// </summary>
    /// <param name="kind">The kind of the database.</param>
    /// <returns><c>MongoDB</c>, <c>PostgreSql</c> or <c>MsSql</c>.</returns>
    public static string StorageTypeOf(string kind) => kind switch
    {
        StorageKinds.PostgreSql => "PostgreSql",
        StorageKinds.SqlServer => "MsSql",
        _ => "MongoDB"
    };

    /// <summary>
    /// Gets the Chronicle sink type of a sink kind.
    /// </summary>
    /// <param name="kind">The kind of the sink.</param>
    /// <returns><c>MongoDB</c>, <c>SQL</c> or <c>Neo4j</c>.</returns>
    public static string SinkTypeOf(string kind) => kind switch
    {
        StorageKinds.PostgreSql or StorageKinds.SqlServer => "SQL",
        StorageKinds.Neo4j => "Neo4j",
        _ => "MongoDB"
    };

    static string FileOf(string setting) => $"{ConnectionsDirectory}/{setting}";

    static Graph? GraphOf(ProductionSpec spec, ProductionNames names) => spec.Graph.Mode switch
    {
        ProductionModes.Managed => new(names.Neo4j, ProductionGraph.ManagedUserName, "Single", null),
        ProductionModes.External when spec.Graph.Edition == GraphEditions.Enterprise =>
            new(spec.Graph.CredentialsSecretName, spec.Graph.UserName, "PerNamespace", string.IsNullOrWhiteSpace(spec.Graph.DatabasePrefix) ? names.Production : spec.Graph.DatabasePrefix),
        ProductionModes.External => new(spec.Graph.CredentialsSecretName, spec.Graph.UserName, "Single", null),
        _ => null
    };

    static void SetGraph(Dictionary<string, string> variables, string setting, Graph graph)
    {
        variables[$"{setting}ServerFile"] = FileOf($"{Neo4j}Server");
        variables[$"{setting}PasswordFile"] = FileOf($"{Neo4j}Password");
        variables[$"{setting}UserName"] = graph.UserName;
        variables[$"{setting}DatabaseMode"] = graph.DatabaseMode;
        if (graph.DatabasePrefix is { } prefix)
        {
            variables[$"{setting}DatabasePrefix"] = prefix;
        }
    }

    sealed record Graph(string SecretName, string UserName, string DatabaseMode, string? DatabasePrefix);
}
