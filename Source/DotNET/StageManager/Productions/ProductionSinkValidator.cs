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
            problems.AddRange(SinkProblems(sink, spec.Graph, seen));
        }

        problems.AddRange(DefaultProblems(sinks, new("readModels", sinks.ReadModels, Graph: false)));
        problems.AddRange(DefaultProblems(sinks, new("graphs", sinks.Graphs, Graph: true)));
    }

    static IEnumerable<ProductionProblem> About(string sink, string? message) => message is null ? [] : [new(message, sink)];

    static IEnumerable<ProductionProblem> SinkProblems(ProductionSinkConfiguration sink, ProductionGraph graph, HashSet<string> seen) =>
        NameProblem(sink.Name, seen) is { } nameProblem ? About(sink.Name, nameProblem) : ProblemsOfKnownName(sink, graph);

    static IEnumerable<ProductionProblem> ProblemsOfKnownName(ProductionSinkConfiguration sink, ProductionGraph graph) => KindProblem(sink) switch
    {
        { } kindProblem => About(sink.Name, kindProblem),
        _ => [.. About(sink.Name, SecretNameProblem(sink)), .. ProblemsOfKind(sink, graph)]
    };

    static IEnumerable<ProductionProblem> ProblemsOfKind(ProductionSinkConfiguration sink, ProductionGraph graph) =>
        sink.Kind == StorageKinds.Neo4j ? About(sink.Name, GraphSinkProblem(sink, graph)) : StorageSinkProblems(sink);

    static string? NameProblem(string name, HashSet<string> seen) => name switch
    {
        _ when !SinkName.IsMatch(name) => $"The sink name '{name}' may only hold letters and digits, separated by single hyphens or underscores.",
        _ when !seen.Add(name) => $"The sink '{name}' is configured twice.",
        _ => null
    };

    static string? KindProblem(ProductionSinkConfiguration sink) => IsKnownKind(sink)
        ? null
        : $"The sink '{sink.Name}' has the unknown kind '{sink.Kind}'. Use one of {string.Join(", ", StorageKinds.Sinks)}.";

    static bool IsKnownKind(ProductionSinkConfiguration sink) => StorageKinds.Sinks.Contains(sink.Kind, StringComparer.Ordinal);

    static string? SecretNameProblem(ProductionSinkConfiguration sink) => sink.ConnectionSecretRef is { } secret && string.IsNullOrWhiteSpace(secret.Name)
        ? $"The sink '{sink.Name}' has a connectionSecretRef without a name."
        : null;

    static string? GraphSinkProblem(ProductionSinkConfiguration sink, ProductionGraph graph) =>
        (!string.IsNullOrWhiteSpace(sink.GraphRef), sink.ConnectionSecretRef is not null) switch
        {
            (true, true) or (false, false) => $"The sink '{sink.Name}' is Neo4j and needs exactly one of connectionSecretRef or graphRef.",
            (true, _) when !string.Equals(sink.GraphRef, ProductionValidator.ManagedGraph, StringComparison.Ordinal) =>
                $"The sink '{sink.Name}' has graphRef '{sink.GraphRef}'. The only graph of a Production is '{ProductionValidator.ManagedGraph}'.",
            (true, _) when graph.Mode != ProductionModes.Managed => $"The sink '{sink.Name}' uses the managed graph, but graph.mode is {graph.Mode}.",
            _ => null
        };

    static IEnumerable<ProductionProblem> StorageSinkProblems(ProductionSinkConfiguration sink) =>
    [
        .. About(sink.Name, string.IsNullOrWhiteSpace(sink.GraphRef) ? null : $"The sink '{sink.Name}' is {sink.Kind}: graphRef is for Neo4j sinks only."),
        .. About(sink.Name, sink.ConnectionSecretRef is null ? $"The sink '{sink.Name}' is {sink.Kind} and needs a connectionSecretRef." : null)
    ];

    static IEnumerable<ProductionProblem> DefaultProblems(ProductionSinks sinks, DefaultSink setting) =>
        string.IsNullOrWhiteSpace(setting.Name)
            ? []
            : DefaultProblems(setting, sinks.Configurations.FirstOrDefault(sink => string.Equals(sink.Name, setting.Name, StringComparison.OrdinalIgnoreCase)));

    static IEnumerable<ProductionProblem> DefaultProblems(DefaultSink setting, ProductionSinkConfiguration? configuration) => configuration switch
    {
        null => About(setting.Name, $"sinks.{setting.Setting} names the sink '{setting.Name}', which is not configured."),
        _ => About(configuration.Name, DefaultKindProblem(configuration, setting))
    };

    static string? DefaultKindProblem(ProductionSinkConfiguration configuration, DefaultSink setting) => (IsKnownKind(configuration), configuration.Kind == StorageKinds.Neo4j, setting.Graph) switch
    {
        (true, false, true) => $"sinks.graphs names the sink '{setting.Name}', which is {configuration.Kind} and cannot store graphs.",
        (true, true, false) => $"sinks.readModels names the sink '{setting.Name}', which is Neo4j and only stores graphs.",
        _ => null
    };

    /// <summary>
    /// A default sink setting of a Production.
    /// </summary>
    /// <param name="Setting">The name of the setting under sinks, as it is written in a message.</param>
    /// <param name="Name">The name of the sink the setting names; blank when it names none.</param>
    /// <param name="Graph">Whether the setting is for graphs rather than read models.</param>
    readonly record struct DefaultSink(string Setting, string Name, bool Graph);
}
