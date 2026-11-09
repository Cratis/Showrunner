// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// Finds what is wrong with the spec of a <see cref="Production"/> before anything is built from it. Every problem with a named sink names that sink.
/// </summary>
public static partial class ProductionValidator
{
    /// <summary>
    /// The graph reference a Neo4j sink uses for the Production's own managed graph.
    /// </summary>
    public const string ManagedGraph = "managed";

    static readonly int[] _memberCounts = [1, 3, 5];
    static readonly string[] _databaseModes = [ProductionModes.Managed, ProductionModes.External];
    static readonly string[] _graphModes = [ProductionModes.None, ProductionModes.Managed, ProductionModes.External];

    [System.Text.RegularExpressions.GeneratedRegex("^[A-Za-z0-9]+([_-][A-Za-z0-9]+)*$", System.Text.RegularExpressions.RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial System.Text.RegularExpressions.Regex SinkName { get; }

    /// <summary>
    /// Validates a Production.
    /// </summary>
    /// <param name="spec">The spec of the Production.</param>
    /// <returns>The messages of the problems, empty when the spec is valid.</returns>
    public static IReadOnlyList<string> Validate(ProductionSpec spec) => [.. ValidateEach(spec).Select(problem => problem.Message)];

    /// <summary>
    /// Validates a Production, keeping which named sink each problem is about.
    /// </summary>
    /// <param name="spec">The spec of the Production.</param>
    /// <returns>The problems, empty when the spec is valid.</returns>
    public static IReadOnlyList<ProductionProblem> ValidateEach(ProductionSpec spec)
    {
        var problems = new List<ProductionProblem>();
        ValidateChronicle(spec, problems);
        ValidateDatabase(spec.Database, problems);
        ValidateGraph(spec.Graph, problems);
        ValidateSinks(spec, problems);
        return problems;
    }

    static void ValidateChronicle(ProductionSpec spec, List<ProductionProblem> problems)
    {
        if (string.IsNullOrWhiteSpace(spec.Version))
        {
            problems.Add(new("version is required: the tag of the cratis/chronicle image to run."));
        }

        if (spec.Replicas < 1)
        {
            problems.Add(new($"replicas must be at least 1, not {spec.Replicas}."));
        }
    }

    static void ValidateDatabase(ProductionDatabase database, List<ProductionProblem> problems)
    {
        if (!StorageKinds.Database.Contains(database.Kind, StringComparer.Ordinal))
        {
            problems.Add(new($"database.kind '{database.Kind}' is unknown. Use one of {string.Join(", ", StorageKinds.Database)}."));
            return;
        }

        if (!_databaseModes.Contains(database.Mode, StringComparer.Ordinal))
        {
            problems.Add(new($"database.mode '{database.Mode}' is unknown. Use one of {string.Join(", ", _databaseModes)}."));
            return;
        }

        var external = database.IsExternal();
        if (StorageKinds.IsSql(database.Kind) && !external)
        {
            problems.Add(new($"database.kind {database.Kind} needs database.mode External: only MongoDB can be managed."));
        }

        if (external && string.IsNullOrWhiteSpace(database.ConnectionSecretName))
        {
            problems.Add(new("database.connectionSecretName is required for an External database."));
        }

        if (!external && !_memberCounts.Contains(database.Members))
        {
            problems.Add(new($"database.members must be 1, 3 or 5 for a Managed database, not {database.Members}."));
        }
    }

    static void ValidateGraph(ProductionGraph graph, List<ProductionProblem> problems)
    {
        if (!_graphModes.Contains(graph.Mode, StringComparer.Ordinal))
        {
            problems.Add(new($"graph.mode '{graph.Mode}' is unknown. Use one of {string.Join(", ", _graphModes)}."));
            return;
        }

        if (!GraphEditions.All.Contains(graph.Edition, StringComparer.Ordinal))
        {
            problems.Add(new($"graph.edition '{graph.Edition}' is unknown. Use one of {string.Join(", ", GraphEditions.All)}."));
            return;
        }

        if (graph.Mode == ProductionModes.Managed && graph.Edition != GraphEditions.Community)
        {
            problems.Add(new("graph.edition must be Community for a Managed graph: Stage Manager runs Neo4j Community only."));
        }

        if (graph.Mode == ProductionModes.External && string.IsNullOrWhiteSpace(graph.CredentialsSecretName))
        {
            problems.Add(new("graph.credentialsSecretName is required for an External graph."));
        }
    }

    static void ValidateSinks(ProductionSpec spec, List<ProductionProblem> problems)
    {
        var sinks = spec.Sinks;
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var sink in sinks.Configurations)
        {
            if (!SinkName.IsMatch(sink.Name))
            {
                problems.Add(new($"The sink name '{sink.Name}' may only hold letters and digits, separated by single hyphens or underscores.", sink.Name));
                continue;
            }

            if (!seen.Add(sink.Name))
            {
                problems.Add(new($"The sink '{sink.Name}' is configured twice.", sink.Name));
                continue;
            }

            ValidateSink(sink, spec.Graph, problems);
        }

        ValidateDefault(sinks, "readModels", sinks.ReadModels, graph: false, problems);
        ValidateDefault(sinks, "graphs", sinks.Graphs, graph: true, problems);
    }

    static void ValidateSink(ProductionSinkConfiguration sink, ProductionGraph graph, List<ProductionProblem> problems)
    {
        if (!StorageKinds.Sinks.Contains(sink.Kind, StringComparer.Ordinal))
        {
            problems.Add(new($"The sink '{sink.Name}' has the unknown kind '{sink.Kind}'. Use one of {string.Join(", ", StorageKinds.Sinks)}.", sink.Name));
            return;
        }

        var hasSecret = sink.ConnectionSecretRef is not null;
        if (hasSecret && string.IsNullOrWhiteSpace(sink.ConnectionSecretRef!.Name))
        {
            problems.Add(new($"The sink '{sink.Name}' has a connectionSecretRef without a name.", sink.Name));
        }

        var hasGraph = !string.IsNullOrWhiteSpace(sink.GraphRef);
        if (sink.Kind == StorageKinds.Neo4j)
        {
            if (hasSecret == hasGraph)
            {
                problems.Add(new($"The sink '{sink.Name}' is Neo4j and needs exactly one of connectionSecretRef or graphRef.", sink.Name));
            }
            else if (hasGraph && !string.Equals(sink.GraphRef, ManagedGraph, StringComparison.Ordinal))
            {
                problems.Add(new($"The sink '{sink.Name}' has graphRef '{sink.GraphRef}'. The only graph of a Production is '{ManagedGraph}'.", sink.Name));
            }
            else if (hasGraph && graph.Mode != ProductionModes.Managed)
            {
                problems.Add(new($"The sink '{sink.Name}' uses the managed graph, but graph.mode is {graph.Mode}.", sink.Name));
            }

            return;
        }

        if (hasGraph)
        {
            problems.Add(new($"The sink '{sink.Name}' is {sink.Kind}: graphRef is for Neo4j sinks only.", sink.Name));
        }

        if (!hasSecret)
        {
            problems.Add(new($"The sink '{sink.Name}' is {sink.Kind} and needs a connectionSecretRef.", sink.Name));
        }
    }

    static void ValidateDefault(ProductionSinks sinks, string setting, string name, bool graph, List<ProductionProblem> problems)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        var configuration = sinks.Configurations.FirstOrDefault(sink => string.Equals(sink.Name, name, StringComparison.OrdinalIgnoreCase));
        if (configuration is null)
        {
            problems.Add(new($"sinks.{setting} names the sink '{name}', which is not configured.", name));
            return;
        }

        if (!StorageKinds.Sinks.Contains(configuration.Kind, StringComparer.Ordinal))
        {
            return;
        }

        var isGraph = configuration.Kind == StorageKinds.Neo4j;
        if (graph && !isGraph)
        {
            problems.Add(new($"sinks.graphs names the sink '{name}', which is {configuration.Kind} and cannot store graphs.", configuration.Name));
        }
        else if (!graph && isGraph)
        {
            problems.Add(new($"sinks.readModels names the sink '{name}', which is Neo4j and only stores graphs.", configuration.Name));
        }
    }
}
