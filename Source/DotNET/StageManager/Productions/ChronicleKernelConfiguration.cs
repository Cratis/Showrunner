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
/// A Neo4j <c>server</c> has no <c>…File</c> setting in the kernel, so the file mounted for it is not read yet.
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
    /// <returns>The kernel configuration.</returns>
    public static ChronicleKernelConfiguration Build(ProductionSpec spec)
    {
        var variables = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [StorageType] = StorageTypeOf(spec.Database.Kind)
        };
        var sources = new List<SecretSource>();

        // A managed database is the Production's own MongoDB; its connection comes with it.
        if (string.Equals(spec.Database.Mode, "External", StringComparison.Ordinal))
        {
            sources.Add(new(spec.Database.ConnectionSecretName, new Dictionary<string, string> { [ProductionSecretReference.DefaultKey] = StorageConnection }));
            variables[$"{StorageConnection}{FileSuffix}"] = FileOf(StorageConnection);
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

            // A Neo4j sink on the Production's own graph gets its server from that graph.
            if (sink.ConnectionSecretRef is { } reference)
            {
                var connection = sink.Kind == StorageKinds.Neo4j ? $"{setting}Server" : $"{setting}ConnectionDetails";
                sources.Add(new(reference.Name, new Dictionary<string, string> { [reference.KeyOrDefault()] = connection }));

                // The kernel has no file form of a Neo4j server.
                if (sink.Kind != StorageKinds.Neo4j)
                {
                    variables[$"{connection}{FileSuffix}"] = FileOf(connection);
                }
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
}
