// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// Finds what is wrong with the spec of a <see cref="Production"/> before anything is built from it. Every problem with a named sink names that sink.
/// </summary>
public static class ProductionValidator
{
    /// <summary>
    /// The graph reference a Neo4j sink uses for the Production's own managed graph.
    /// </summary>
    public const string ManagedGraph = "managed";

    static readonly int[] _memberCounts = [1, 3, 5];
    static readonly string[] _databaseModes = [ProductionModes.Managed, ProductionModes.External];
    static readonly string[] _graphModes = [ProductionModes.None, ProductionModes.Managed, ProductionModes.External];

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
        ProductionSinkValidator.Validate(spec, problems);
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

    static bool Unknown(IEnumerable<string> known, string value) => !known.Contains(value, StringComparer.Ordinal);

    static void ValidateDatabase(ProductionDatabase database, List<ProductionProblem> problems)
    {
        if (UnknownDatabaseSetting(database, problems))
        {
            return;
        }

        ValidateDatabaseShape(database, problems);
    }

    static bool UnknownDatabaseSetting(ProductionDatabase database, List<ProductionProblem> problems)
    {
        if (Unknown(StorageKinds.Database, database.Kind))
        {
            problems.Add(new($"database.kind '{database.Kind}' is unknown. Use one of {string.Join(", ", StorageKinds.Database)}."));
            return true;
        }

        if (Unknown(_databaseModes, database.Mode))
        {
            problems.Add(new($"database.mode '{database.Mode}' is unknown. Use one of {string.Join(", ", _databaseModes)}."));
            return true;
        }

        return false;
    }

    static void ValidateDatabaseShape(ProductionDatabase database, List<ProductionProblem> problems)
    {
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
        if (Unknown(_graphModes, graph.Mode))
        {
            problems.Add(new($"graph.mode '{graph.Mode}' is unknown. Use one of {string.Join(", ", _graphModes)}."));
            return;
        }

        if (Unknown(GraphEditions.All, graph.Edition))
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
}
