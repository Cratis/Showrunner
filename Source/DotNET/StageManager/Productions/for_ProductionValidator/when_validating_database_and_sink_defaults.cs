// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionValidator;

public class when_validating_database_and_sink_defaults : Specification
{
    readonly Dictionary<string, IReadOnlyList<ProductionProblem>> _problems = new(StringComparer.Ordinal);

    static IReadOnlyList<ProductionProblem> OnDatabase(Action<ProductionDatabase> change)
    {
        var spec = ProductionSamples.External();
        change(spec.Database);
        return ProductionValidator.ValidateEach(spec);
    }

    static IReadOnlyList<ProductionProblem> OnSinks(Action<ProductionSpec> change)
    {
        var spec = ProductionSamples.WithSinks();
        change(spec);
        return ProductionValidator.ValidateEach(spec);
    }

    static string Text(IEnumerable<ProductionProblem> problems) => string.Join('\n', problems.Select(problem => $"{problem.Message} [{problem.Sink}]"));

    static void ShouldBeProblems(IReadOnlyList<ProductionProblem> actual, params ProductionProblem[] expected) => Text(actual).ShouldEqual(Text(expected));

    static ProductionProblem[] Only(string message, string? sink = null) => [new(message, sink)];

    void Because()
    {
        _problems["unknown_kind"] = OnDatabase(database =>
        {
            database.Kind = "Oracle";
            database.Mode = "Hosted";
            database.Members = 2;
        });
        _problems["unknown_mode"] = OnDatabase(database =>
        {
            database.Mode = "Hosted";
            database.Members = 2;
        });
        _problems["sql_managed"] = OnDatabase(database =>
        {
            database.Kind = StorageKinds.PostgreSql;
            database.Mode = ProductionModes.Managed;
        });
        _problems["sql_external_without_secret"] = OnDatabase(database =>
        {
            database.Kind = StorageKinds.SqlServer;
            database.ConnectionSecretName = string.Empty;
        });
        _problems["external_blank_secret"] = OnDatabase(database => database.ConnectionSecretName = "  ");
        _problems["managed_two"] = OnDatabase(database =>
        {
            database.Mode = ProductionModes.Managed;
            database.Members = 2;
        });
        _problems["managed_four"] = OnDatabase(database =>
        {
            database.Mode = ProductionModes.Managed;
            database.Members = 4;
        });
        _problems["managed_five"] = OnDatabase(database =>
        {
            database.Mode = ProductionModes.Managed;
            database.Members = 5;
        });
        _problems["managed_one"] = OnDatabase(database =>
        {
            database.Mode = ProductionModes.Managed;
            database.Members = 1;
        });
        _problems["sql_managed_two"] = OnDatabase(database =>
        {
            database.Kind = StorageKinds.PostgreSql;
            database.Mode = ProductionModes.Managed;
            database.Members = 2;
        });
        _problems["blank_default"] = OnSinks(spec =>
        {
            spec.Sinks.ReadModels = " ";
            spec.Sinks.Graphs = string.Empty;
        });
        _problems["not_configured"] = OnSinks(spec => spec.Sinks.ReadModels = "missing");
        _problems["other_case"] = OnSinks(spec =>
        {
            spec.Sinks.ReadModels = "REPORTING";
            spec.Sinks.Graphs = "PEOPLE";
        });
        _problems["unknown_kind_default"] = OnSinks(spec =>
        {
            spec.Sinks.Configurations[0] = new() { Name = "reporting", Kind = "Oracle" };
            spec.Sinks.ReadModels = "reporting";
        });
        _problems["graphs_on_storage"] = OnSinks(spec => spec.Sinks.Graphs = "WAREHOUSE");
        _problems["read_models_on_neo4j"] = OnSinks(spec => spec.Sinks.ReadModels = "PEOPLE");
        _problems["valid_defaults"] = OnSinks(_ => { });
        _problems["both_defaults_bad"] = OnSinks(spec =>
        {
            spec.Sinks.Configurations[2].ConnectionSecretRef = null;
            spec.Sinks.ReadModels = "people";
            spec.Sinks.Graphs = "warehouse";
        });
        _problems["all_sections"] = OnSinks(spec =>
        {
            spec.Database.Kind = "Oracle";
            spec.Graph.Mode = "Hosted";
            spec.Sinks.Configurations.RemoveAt(3);
            spec.Sinks.Graphs = string.Empty;
            spec.Sinks.Configurations.Add(new() { Name = "-bad", Kind = StorageKinds.MongoDB });
            spec.Sinks.ReadModels = "missing";
        });
    }

    [Fact] void should_report_only_an_unknown_database_kind() => ShouldBeProblems(_problems["unknown_kind"], Only("database.kind 'Oracle' is unknown. Use one of MongoDB, PostgreSql, SqlServer."));
    [Fact] void should_report_only_an_unknown_database_mode_of_a_known_kind() => ShouldBeProblems(_problems["unknown_mode"], Only("database.mode 'Hosted' is unknown. Use one of Managed, External."));
    [Fact] void should_refuse_a_managed_sql_database() => ShouldBeProblems(_problems["sql_managed"], Only("database.kind PostgreSql needs database.mode External: only MongoDB can be managed."));
    [Fact] void should_only_require_the_secret_of_an_external_sql_database() => ShouldBeProblems(_problems["sql_external_without_secret"], Only("database.connectionSecretName is required for an External database."));
    [Fact] void should_require_a_secret_that_is_not_blank() => ShouldBeProblems(_problems["external_blank_secret"], Only("database.connectionSecretName is required for an External database."));
    [Fact] void should_refuse_two_members() => ShouldBeProblems(_problems["managed_two"], Only("database.members must be 1, 3 or 5 for a Managed database, not 2."));
    [Fact] void should_refuse_four_members() => ShouldBeProblems(_problems["managed_four"], Only("database.members must be 1, 3 or 5 for a Managed database, not 4."));
    [Fact] void should_accept_five_members() => _problems["managed_five"].Count.ShouldEqual(0);
    [Fact] void should_accept_one_member() => _problems["managed_one"].Count.ShouldEqual(0);
    [Fact] void should_report_the_sql_mode_before_the_members() => ShouldBeProblems(_problems["sql_managed_two"], 
    [
        new("database.kind PostgreSql needs database.mode External: only MongoDB can be managed."),
        new("database.members must be 1, 3 or 5 for a Managed database, not 2.")
    ]);
    [Fact] void should_accept_blank_defaults() => _problems["blank_default"].Count.ShouldEqual(0);
    [Fact] void should_name_a_default_that_is_not_configured() => ShouldBeProblems(_problems["not_configured"], Only("sinks.readModels names the sink 'missing', which is not configured.", "missing"));
    [Fact] void should_match_a_default_without_regard_to_case() => _problems["other_case"].Count.ShouldEqual(0);
    [Fact] void should_only_report_the_unknown_kind_of_a_default_sink_itself() => ShouldBeProblems(_problems["unknown_kind_default"], Only("The sink 'reporting' has the unknown kind 'Oracle'. Use one of MongoDB, PostgreSql, SqlServer, Neo4j.", "reporting"));
    [Fact] void should_refuse_a_graphs_default_that_cannot_store_graphs() => ShouldBeProblems(_problems["graphs_on_storage"], Only("sinks.graphs names the sink 'WAREHOUSE', which is SqlServer and cannot store graphs.", "warehouse"));
    [Fact] void should_refuse_a_read_models_default_that_only_stores_graphs() => ShouldBeProblems(_problems["read_models_on_neo4j"], Only("sinks.readModels names the sink 'PEOPLE', which is Neo4j and only stores graphs.", "people"));
    [Fact] void should_accept_valid_defaults() => _problems["valid_defaults"].Count.ShouldEqual(0);
    [Fact] void should_report_the_sink_problems_before_the_read_models_default_before_the_graphs_default() => ShouldBeProblems(_problems["both_defaults_bad"], 
    [
        new("The sink 'documents' is MongoDB and needs a connectionSecretRef.", "documents"),
        new("sinks.readModels names the sink 'people', which is Neo4j and only stores graphs.", "people"),
        new("sinks.graphs names the sink 'warehouse', which is SqlServer and cannot store graphs.", "warehouse")
    ]);
    [Fact] void should_report_the_database_before_the_graph_before_the_sinks() => ShouldBeProblems(_problems["all_sections"], 
    [
        new("database.kind 'Oracle' is unknown. Use one of MongoDB, PostgreSql, SqlServer."),
        new("graph.mode 'Hosted' is unknown. Use one of None, Managed, External."),
        new("The sink name '-bad' may only hold letters and digits, separated by single hyphens or underscores.", "-bad"),
        new("sinks.readModels names the sink 'missing', which is not configured.", "missing")
    ]);
}
#endif
