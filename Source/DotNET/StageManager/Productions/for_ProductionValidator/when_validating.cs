// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionValidator;

public class when_validating : Specification
{
    static IReadOnlyList<string> Validate(Action<ProductionSpec> change)
    {
        var spec = ProductionSamples.WithSinks();
        change(spec);
        return ProductionValidator.Validate(spec);
    }

    static bool Mentions(IReadOnlyList<string> problems, string text) => problems.Any(problem => problem.Contains(text, StringComparison.Ordinal));

    [Fact] void should_accept_a_managed_mongodb_without_sinks() => ProductionValidator.Validate(ProductionSamples.Managed()).Count.ShouldEqual(0);
    [Fact] void should_accept_named_sinks_of_every_kind() => ProductionValidator.Validate(ProductionSamples.WithSinks()).Count.ShouldEqual(0);
    [Fact] void should_accept_an_external_postgresql_database() => Validate(spec => spec.Database.Kind = StorageKinds.PostgreSql).Count.ShouldEqual(0);
    [Fact] void should_refuse_an_unknown_database_kind() => Mentions(Validate(spec => spec.Database.Kind = "Oracle"), "database.kind 'Oracle' is unknown").ShouldBeTrue();
    [Fact] void should_refuse_a_managed_sql_database() => Mentions(Validate(spec => spec.Database = new() { Kind = StorageKinds.SqlServer, Mode = "Managed" }), "needs database.mode External").ShouldBeTrue();
    [Fact] void should_require_the_secret_of_an_external_database() => Mentions(Validate(spec => spec.Database.ConnectionSecretName = string.Empty), "database.connectionSecretName").ShouldBeTrue();
    [Fact] void should_refuse_a_duplicate_name_without_regard_to_case() => Mentions(Validate(spec => spec.Sinks.Configurations.Add(new() { Name = "Reporting", Kind = StorageKinds.PostgreSql, ConnectionSecretRef = new() { Name = "other" } })), "The sink 'Reporting' is configured twice").ShouldBeTrue();
    [Fact] void should_refuse_an_unknown_kind_naming_the_sink() => Mentions(Validate(spec => spec.Sinks.Configurations[1].Kind = "Oracle"), "The sink 'warehouse' has the unknown kind 'Oracle'").ShouldBeTrue();
    [Fact] void should_refuse_a_name_that_cannot_be_a_setting() => Mentions(Validate(spec => spec.Sinks.Configurations[2].Name = "my__sink"), "The sink name 'my__sink'").ShouldBeTrue();
    [Fact] void should_refuse_read_models_naming_a_missing_sink() => Mentions(Validate(spec => spec.Sinks.ReadModels = "primary"), "sinks.readModels names the sink 'primary', which is not configured").ShouldBeTrue();
    [Fact] void should_refuse_graphs_naming_a_missing_sink() => Mentions(Validate(spec => spec.Sinks.Graphs = "graph"), "sinks.graphs names the sink 'graph', which is not configured").ShouldBeTrue();
    [Fact] void should_refuse_neo4j_for_read_models() => Mentions(Validate(spec => spec.Sinks.ReadModels = "people"), "sinks.readModels names the sink 'people', which is Neo4j").ShouldBeTrue();
    [Fact] void should_refuse_sql_for_graphs() => Mentions(Validate(spec => spec.Sinks.Graphs = "reporting"), "sinks.graphs names the sink 'reporting', which is PostgreSql").ShouldBeTrue();
    [Fact] void should_refuse_mongodb_for_graphs() => Mentions(Validate(spec => spec.Sinks.Graphs = "documents"), "sinks.graphs names the sink 'documents', which is MongoDB").ShouldBeTrue();
    [Fact] void should_require_a_secret_for_a_sql_sink() => Mentions(Validate(spec => spec.Sinks.Configurations[0].ConnectionSecretRef = null), "The sink 'reporting' is PostgreSql and needs a connectionSecretRef").ShouldBeTrue();
    [Fact] void should_refuse_a_graph_for_a_sql_sink() => Mentions(Validate(spec => spec.Sinks.Configurations[0].GraphRef = "managed"), "graphRef is for Neo4j sinks only").ShouldBeTrue();
    [Fact] void should_refuse_a_neo4j_sink_with_both_a_graph_and_a_secret() => Mentions(Validate(spec => spec.Sinks.Configurations[3].ConnectionSecretRef = new() { Name = "people-graph" }), "The sink 'people' is Neo4j and needs exactly one").ShouldBeTrue();
    [Fact] void should_refuse_a_neo4j_sink_with_neither_a_graph_nor_a_secret() => Mentions(Validate(spec => spec.Sinks.Configurations[3].GraphRef = null), "The sink 'people' is Neo4j and needs exactly one").ShouldBeTrue();
}
#endif
