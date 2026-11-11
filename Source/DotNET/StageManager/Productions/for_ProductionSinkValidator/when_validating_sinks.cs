// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionSinkValidator;

public class when_validating_sinks : Specification
{
    const string NameRule = "may only hold letters and digits, separated by single hyphens or underscores.";
    const string KindList = "Use one of MongoDB, PostgreSql, SqlServer, Neo4j.";

    readonly Dictionary<string, string> _problems = new(StringComparer.Ordinal);

    static string Validate(Action<ProductionSpec> change)
    {
        var spec = ProductionSamples.WithSinks();
        change(spec);
        var problems = new List<ProductionProblem>();
        ProductionSinkValidator.Validate(spec, problems);
        return string.Join('\n', problems.Select(problem => $"{problem.Message} [{problem.Sink}]"));
    }

    static ProductionSinkConfiguration Sink(string name, string kind, string? secret = null) => new()
    {
        Name = name,
        Kind = kind,
        ConnectionSecretRef = secret is null ? null : new() { Name = secret }
    };

    void Because()
    {
        _problems["valid"] = Validate(_ => { });
        _problems["invalid_name"] = Validate(spec => spec.Sinks.Configurations[0].Name = "my__sink");
        _problems["duplicate"] = Validate(spec => spec.Sinks.Configurations.Add(Sink("Reporting", StorageKinds.PostgreSql, "other")));
        _problems["invalid_names_twice"] = Validate(spec =>
        {
            spec.Sinks.Configurations.Add(Sink("-x", StorageKinds.PostgreSql, "a"));
            spec.Sinks.Configurations.Add(Sink("-x", StorageKinds.PostgreSql, "a"));
        });
        _problems["unknown_kind"] = Validate(spec => spec.Sinks.Configurations[1] = Sink("warehouse", "Oracle"));
        _problems["blank_secret_name"] = Validate(spec => spec.Sinks.Configurations[0].ConnectionSecretRef = new() { Name = " " });
        _problems["neo4j_blank_secret_name"] = Validate(spec => spec.Sinks.Configurations[4].ConnectionSecretRef = new() { Name = string.Empty });
        _problems["neo4j_both"] = Validate(spec => spec.Sinks.Configurations[3].ConnectionSecretRef = new() { Name = "s" });
        _problems["neo4j_neither"] = Validate(spec => spec.Sinks.Configurations[3].GraphRef = " ");
        _problems["neo4j_other_graph"] = Validate(spec => spec.Sinks.Configurations[3].GraphRef = "other");
        _problems["neo4j_graph_not_managed"] = Validate(spec => spec.Graph.Mode = ProductionModes.External);
        _problems["storage_graph_ref"] = Validate(spec => spec.Sinks.Configurations[0].GraphRef = "managed");
        _problems["storage_no_secret"] = Validate(spec => spec.Sinks.Configurations[2].ConnectionSecretRef = null);
        _problems["storage_both"] = Validate(spec =>
        {
            spec.Sinks.Configurations[0].GraphRef = "managed";
            spec.Sinks.Configurations[0].ConnectionSecretRef = null;
        });
        _problems["default_missing"] = Validate(spec =>
        {
            spec.Sinks.ReadModels = "nope";
            spec.Sinks.Graphs = "nada";
        });
        _problems["default_neo4j_read"] = Validate(spec => spec.Sinks.ReadModels = "organization");
        _problems["default_storage_graph"] = Validate(spec => spec.Sinks.Graphs = "documents");
        _problems["default_unknown_kind"] = Validate(spec =>
        {
            spec.Sinks.Configurations[0] = Sink("reporting", "Oracle");
            spec.Sinks.Graphs = "reporting";
        });
        _problems["defaults_blank"] = Validate(spec =>
        {
            spec.Sinks.ReadModels = " ";
            spec.Sinks.Graphs = string.Empty;
        });
        _problems["order"] = Validate(spec =>
        {
            spec.Sinks.Configurations[0] = Sink("-bad", StorageKinds.PostgreSql, "x");
            spec.Sinks.Configurations[1].ConnectionSecretRef = null;
            spec.Sinks.ReadModels = "people";
            spec.Sinks.Graphs = "documents";
        });
    }

    [Fact] void should_accept_valid_sinks() => _problems["valid"].ShouldEqual(string.Empty);
    [Fact] void should_refuse_an_invalid_name() => _problems["invalid_name"].ShouldEqual($"The sink name 'my__sink' {NameRule} [my__sink]\nsinks.readModels names the sink 'reporting', which is not configured. [reporting]");
    [Fact] void should_refuse_a_duplicate_name_ignoring_case() => _problems["duplicate"].ShouldEqual("The sink 'Reporting' is configured twice. [Reporting]");
    [Fact] void should_not_remember_an_invalid_name_as_seen() => _problems["invalid_names_twice"].ShouldEqual($"The sink name '-x' {NameRule} [-x]\nThe sink name '-x' {NameRule} [-x]");
    [Fact] void should_refuse_an_unknown_kind_and_check_nothing_else_of_it() => _problems["unknown_kind"].ShouldEqual($"The sink 'warehouse' has the unknown kind 'Oracle'. {KindList} [warehouse]");
    [Fact] void should_refuse_a_secret_without_a_name() => _problems["blank_secret_name"].ShouldEqual("The sink 'reporting' has a connectionSecretRef without a name. [reporting]");
    [Fact] void should_refuse_a_neo4j_secret_without_a_name() => _problems["neo4j_blank_secret_name"].ShouldEqual("The sink 'organization' has a connectionSecretRef without a name. [organization]");
    [Fact] void should_refuse_neo4j_with_both() => _problems["neo4j_both"].ShouldEqual("The sink 'people' is Neo4j and needs exactly one of connectionSecretRef or graphRef. [people]");
    [Fact] void should_refuse_neo4j_with_neither() => _problems["neo4j_neither"].ShouldEqual("The sink 'people' is Neo4j and needs exactly one of connectionSecretRef or graphRef. [people]");
    [Fact] void should_refuse_a_graph_that_is_not_the_managed_one() => _problems["neo4j_other_graph"].ShouldEqual("The sink 'people' has graphRef 'other'. The only graph of a Production is 'managed'. [people]");
    [Fact] void should_refuse_the_managed_graph_when_the_graph_is_not_managed() => _problems["neo4j_graph_not_managed"].ShouldEqual("The sink 'people' uses the managed graph, but graph.mode is External. [people]");
    [Fact] void should_refuse_a_graph_ref_on_a_storage_sink() => _problems["storage_graph_ref"].ShouldEqual("The sink 'reporting' is PostgreSql: graphRef is for Neo4j sinks only. [reporting]");
    [Fact] void should_require_a_secret_on_a_storage_sink() => _problems["storage_no_secret"].ShouldEqual("The sink 'documents' is MongoDB and needs a connectionSecretRef. [documents]");
    [Fact] void should_report_both_storage_problems_in_order() => _problems["storage_both"].ShouldEqual("The sink 'reporting' is PostgreSql: graphRef is for Neo4j sinks only. [reporting]\nThe sink 'reporting' is PostgreSql and needs a connectionSecretRef. [reporting]");
    [Fact] void should_name_defaults_that_are_not_configured_in_order() => _problems["default_missing"].ShouldEqual("sinks.readModels names the sink 'nope', which is not configured. [nope]\nsinks.graphs names the sink 'nada', which is not configured. [nada]");
    [Fact] void should_refuse_neo4j_as_the_read_model_default() => _problems["default_neo4j_read"].ShouldEqual("sinks.readModels names the sink 'organization', which is Neo4j and only stores graphs. [organization]");
    [Fact] void should_refuse_a_storage_sink_as_the_graph_default() => _problems["default_storage_graph"].ShouldEqual("sinks.graphs names the sink 'documents', which is MongoDB and cannot store graphs. [documents]");
    [Fact] void should_add_no_default_problem_for_an_unknown_kind() => _problems["default_unknown_kind"].ShouldEqual($"The sink 'reporting' has the unknown kind 'Oracle'. {KindList} [reporting]");
    [Fact] void should_ignore_blank_defaults() => _problems["defaults_blank"].ShouldEqual(string.Empty);
    [Fact] void should_report_sinks_before_defaults_in_configuration_order() => _problems["order"].ShouldEqual($"The sink name '-bad' {NameRule} [-bad]\nThe sink 'warehouse' is SqlServer and needs a connectionSecretRef. [warehouse]\nsinks.readModels names the sink 'people', which is Neo4j and only stores graphs. [people]\nsinks.graphs names the sink 'documents', which is MongoDB and cannot store graphs. [documents]");
}
#endif
