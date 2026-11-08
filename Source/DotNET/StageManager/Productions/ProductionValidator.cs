// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// Finds what is wrong with the storage and named sinks of a <see cref="Production"/> before any kernel configuration is built from it. Every problem names the sink it is about.
/// </summary>
public static partial class ProductionValidator
{
    [System.Text.RegularExpressions.GeneratedRegex("^[A-Za-z0-9]+([_-][A-Za-z0-9]+)*$", System.Text.RegularExpressions.RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial System.Text.RegularExpressions.Regex SinkName { get; }

    /// <summary>
    /// Validates the storage and named sinks of a Production.
    /// </summary>
    /// <param name="spec">The spec of the Production.</param>
    /// <returns>The problems, empty when the spec is valid.</returns>
    public static IReadOnlyList<string> Validate(ProductionSpec spec)
    {
        var problems = new List<string>();
        ValidateDatabase(spec.Database, problems);
        ValidateSinks(spec.Sinks, problems);
        return problems;
    }

    static void ValidateDatabase(ProductionDatabase database, List<string> problems)
    {
        if (!StorageKinds.Database.Contains(database.Kind, StringComparer.Ordinal))
        {
            problems.Add($"database.kind '{database.Kind}' is unknown. Use one of {string.Join(", ", StorageKinds.Database)}.");
            return;
        }

        var external = string.Equals(database.Mode, "External", StringComparison.Ordinal);
        if (StorageKinds.IsSql(database.Kind) && !external)
        {
            problems.Add($"database.kind {database.Kind} needs database.mode External: only MongoDB can be managed.");
        }

        if (external && string.IsNullOrWhiteSpace(database.ConnectionSecretName))
        {
            problems.Add("database.connectionSecretName is required for an External database.");
        }
    }

    static void ValidateSinks(ProductionSinks sinks, List<string> problems)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var sink in sinks.Configurations)
        {
            if (!SinkName.IsMatch(sink.Name))
            {
                problems.Add($"The sink name '{sink.Name}' may only hold letters and digits, separated by single hyphens or underscores.");
                continue;
            }

            if (!seen.Add(sink.Name))
            {
                problems.Add($"The sink '{sink.Name}' is configured twice.");
                continue;
            }

            ValidateSink(sink, problems);
        }

        ValidateDefault(sinks, "readModels", sinks.ReadModels, graph: false, problems);
        ValidateDefault(sinks, "graphs", sinks.Graphs, graph: true, problems);
    }

    static void ValidateSink(ProductionSinkConfiguration sink, List<string> problems)
    {
        if (!StorageKinds.Sinks.Contains(sink.Kind, StringComparer.Ordinal))
        {
            problems.Add($"The sink '{sink.Name}' has the unknown kind '{sink.Kind}'. Use one of {string.Join(", ", StorageKinds.Sinks)}.");
            return;
        }

        var hasSecret = sink.ConnectionSecretRef is not null;
        if (hasSecret && string.IsNullOrWhiteSpace(sink.ConnectionSecretRef!.Name))
        {
            problems.Add($"The sink '{sink.Name}' has a connectionSecretRef without a name.");
        }

        var hasGraph = !string.IsNullOrWhiteSpace(sink.GraphRef);
        if (sink.Kind == StorageKinds.Neo4j)
        {
            if (hasSecret == hasGraph)
            {
                problems.Add($"The sink '{sink.Name}' is Neo4j and needs exactly one of connectionSecretRef or graphRef.");
            }

            return;
        }

        if (hasGraph)
        {
            problems.Add($"The sink '{sink.Name}' is {sink.Kind}: graphRef is for Neo4j sinks only.");
        }

        if (!hasSecret)
        {
            problems.Add($"The sink '{sink.Name}' is {sink.Kind} and needs a connectionSecretRef.");
        }
    }

    static void ValidateDefault(ProductionSinks sinks, string setting, string name, bool graph, List<string> problems)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        var configuration = sinks.Configurations.FirstOrDefault(sink => string.Equals(sink.Name, name, StringComparison.OrdinalIgnoreCase));
        if (configuration is null)
        {
            problems.Add($"sinks.{setting} names the sink '{name}', which is not configured.");
            return;
        }

        if (!StorageKinds.Sinks.Contains(configuration.Kind, StringComparer.Ordinal))
        {
            return;
        }

        var isGraph = configuration.Kind == StorageKinds.Neo4j;
        if (graph && !isGraph)
        {
            problems.Add($"sinks.graphs names the sink '{name}', which is {configuration.Kind} and cannot store graphs.");
        }
        else if (!graph && isGraph)
        {
            problems.Add($"sinks.readModels names the sink '{name}', which is Neo4j and only stores graphs.");
        }
    }
}
