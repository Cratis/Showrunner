// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// Finds what is wrong with the named sinks of a <see cref="ProductionSpec"/> and with the defaults that name them. Every
/// problem names the sink it is about.
/// </summary>
internal static partial class ProductionSinkValidator
{
    [System.Text.RegularExpressions.GeneratedRegex("^[A-Za-z0-9]+([_-][A-Za-z0-9]+)*$", System.Text.RegularExpressions.RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial System.Text.RegularExpressions.Regex SinkName { get; }

    /// <summary>
    /// Validates the sinks of a Production and the defaults that name them.
    /// </summary>
    /// <param name="spec">The spec of the Production.</param>
    /// <param name="problems">The problems to add to.</param>
    internal static void Validate(ProductionSpec spec, List<ProductionProblem> problems)
    {
        var sinks = spec.Sinks;
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var sink in sinks.Configurations)
        {
            if (NameProblem(sink.Name, seen) is { } nameProblem)
            {
                problems.Add(new(nameProblem, sink.Name));
                continue;
            }

            ValidateSink(sink, spec.Graph, problems);
        }

        ValidateDefault(sinks, new("readModels", sinks.ReadModels, Graph: false), problems);
        ValidateDefault(sinks, new("graphs", sinks.Graphs, Graph: true), problems);
    }

    static string? NameProblem(string name, HashSet<string> seen) => name switch
    {
        _ when !SinkName.IsMatch(name) => $"The sink name '{name}' may only hold letters and digits, separated by single hyphens or underscores.",
        _ when !seen.Add(name) => $"The sink '{name}' is configured twice.",
        _ => null
    };

    static void ValidateSink(ProductionSinkConfiguration sink, ProductionGraph graph, List<ProductionProblem> problems)
    {
        if (!StorageKinds.Sinks.Contains(sink.Kind, StringComparer.Ordinal))
        {
            problems.Add(new($"The sink '{sink.Name}' has the unknown kind '{sink.Kind}'. Use one of {string.Join(", ", StorageKinds.Sinks)}.", sink.Name));
            return;
        }

        ValidateSecretName(sink, problems);
        if (sink.Kind == StorageKinds.Neo4j)
        {
            ValidateGraphSink(sink, graph, problems);
            return;
        }

        ValidateStorageSink(sink, problems);
    }

    static void ValidateSecretName(ProductionSinkConfiguration sink, List<ProductionProblem> problems)
    {
        if (sink.ConnectionSecretRef is { } secret && string.IsNullOrWhiteSpace(secret.Name))
        {
            problems.Add(new($"The sink '{sink.Name}' has a connectionSecretRef without a name.", sink.Name));
        }
    }

    static void ValidateGraphSink(ProductionSinkConfiguration sink, ProductionGraph graph, List<ProductionProblem> problems)
    {
        var hasGraph = !string.IsNullOrWhiteSpace(sink.GraphRef);
        var problem = (hasGraph, sink.ConnectionSecretRef is not null) switch
        {
            (true, true) or (false, false) => $"The sink '{sink.Name}' is Neo4j and needs exactly one of connectionSecretRef or graphRef.",
            (true, _) when !string.Equals(sink.GraphRef, ProductionValidator.ManagedGraph, StringComparison.Ordinal) =>
                $"The sink '{sink.Name}' has graphRef '{sink.GraphRef}'. The only graph of a Production is '{ProductionValidator.ManagedGraph}'.",
            (true, _) when graph.Mode != ProductionModes.Managed => $"The sink '{sink.Name}' uses the managed graph, but graph.mode is {graph.Mode}.",
            _ => null
        };

        if (problem is not null)
        {
            problems.Add(new(problem, sink.Name));
        }
    }

    static void ValidateStorageSink(ProductionSinkConfiguration sink, List<ProductionProblem> problems)
    {
        if (!string.IsNullOrWhiteSpace(sink.GraphRef))
        {
            problems.Add(new($"The sink '{sink.Name}' is {sink.Kind}: graphRef is for Neo4j sinks only.", sink.Name));
        }

        if (sink.ConnectionSecretRef is null)
        {
            problems.Add(new($"The sink '{sink.Name}' is {sink.Kind} and needs a connectionSecretRef.", sink.Name));
        }
    }

    static void ValidateDefault(ProductionSinks sinks, DefaultSink setting, List<ProductionProblem> problems)
    {
        if (string.IsNullOrWhiteSpace(setting.Name))
        {
            return;
        }

        var configuration = sinks.Configurations.FirstOrDefault(sink => string.Equals(sink.Name, setting.Name, StringComparison.OrdinalIgnoreCase));
        if (configuration is null)
        {
            problems.Add(new($"sinks.{setting.Setting} names the sink '{setting.Name}', which is not configured.", setting.Name));
            return;
        }

        ValidateDefaultKind(configuration, setting, problems);
    }

    static void ValidateDefaultKind(ProductionSinkConfiguration configuration, DefaultSink setting, List<ProductionProblem> problems)
    {
        if (!StorageKinds.Sinks.Contains(configuration.Kind, StringComparer.Ordinal))
        {
            return;
        }

        var isGraph = configuration.Kind == StorageKinds.Neo4j;
        if (setting.Graph && !isGraph)
        {
            problems.Add(new($"sinks.graphs names the sink '{setting.Name}', which is {configuration.Kind} and cannot store graphs.", configuration.Name));
        }
        else if (!setting.Graph && isGraph)
        {
            problems.Add(new($"sinks.readModels names the sink '{setting.Name}', which is Neo4j and only stores graphs.", configuration.Name));
        }
    }

    /// <summary>
    /// A default sink setting of a Production.
    /// </summary>
    /// <param name="Setting">The name of the setting under sinks, as it is written in a message.</param>
    /// <param name="Name">The name of the sink the setting names; blank when it names none.</param>
    /// <param name="Graph">Whether the setting is for graphs rather than read models.</param>
    readonly record struct DefaultSink(string Setting, string Name, bool Graph);
}
