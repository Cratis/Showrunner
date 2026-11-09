// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionValidator;

public class when_validating_the_graph_and_chronicle : Specification
{
    static IReadOnlyList<ProductionProblem> Validate(Action<ProductionSpec> change)
    {
        var spec = ProductionSamples.WithSinks();
        change(spec);
        return ProductionValidator.ValidateEach(spec);
    }

    static bool Mentions(IReadOnlyList<ProductionProblem> problems, string text) => problems.Any(problem => problem.Message.Contains(text, StringComparison.Ordinal));

    [Fact] void should_accept_no_graph() => ProductionValidator.Validate(ProductionSamples.External()).Count.ShouldEqual(0);
    [Fact] void should_accept_an_external_enterprise_graph() => Validate(spec => spec.Graph = new() { Mode = ProductionModes.External, Edition = GraphEditions.Enterprise, CredentialsSecretName = "graph" }).Any(problem => problem.Sink is null).ShouldBeFalse();
    [Fact] void should_require_a_version() => Mentions(Validate(spec => spec.Version = string.Empty), "version is required").ShouldBeTrue();
    [Fact] void should_require_a_replica() => Mentions(Validate(spec => spec.Replicas = 0), "replicas must be at least 1").ShouldBeTrue();
    [Fact] void should_refuse_an_unknown_database_mode() => Mentions(Validate(spec => spec.Database.Mode = "Hosted"), "database.mode 'Hosted' is unknown").ShouldBeTrue();
    [Fact] void should_refuse_two_members() => Mentions(Validate(spec => spec.Database = new() { Mode = ProductionModes.Managed, Members = 2 }), "database.members must be 1, 3 or 5").ShouldBeTrue();
    [Fact] void should_accept_three_members() => ProductionValidator.Validate(new() { Version = "19.148.0", Database = new() { Mode = ProductionModes.Managed, Members = 3 } }).Count.ShouldEqual(0);
    [Fact] void should_refuse_an_unknown_graph_mode() => Mentions(Validate(spec => spec.Graph.Mode = "Hosted"), "graph.mode 'Hosted' is unknown").ShouldBeTrue();
    [Fact] void should_refuse_an_unknown_edition() => Mentions(Validate(spec => spec.Graph.Edition = "Aura"), "graph.edition 'Aura' is unknown").ShouldBeTrue();
    [Fact] void should_refuse_a_managed_enterprise_graph() => Mentions(Validate(spec => spec.Graph.Edition = GraphEditions.Enterprise), "graph.edition must be Community for a Managed graph").ShouldBeTrue();
    [Fact] void should_require_the_secret_of_an_external_graph() => Mentions(Validate(spec => spec.Graph = new() { Mode = ProductionModes.External }), "graph.credentialsSecretName is required").ShouldBeTrue();
    [Fact] void should_refuse_a_sink_on_the_managed_graph_without_one() => Validate(spec => spec.Graph.Mode = ProductionModes.None).Single(problem => problem.Message.Contains("uses the managed graph", StringComparison.Ordinal)).Sink.ShouldEqual("people");
    [Fact] void should_refuse_a_graph_reference_other_than_managed() => Validate(spec => spec.Sinks.Configurations[3].GraphRef = "shared").Single(problem => problem.Message.Contains("graphRef 'shared'", StringComparison.Ordinal)).Sink.ShouldEqual("people");
    [Fact] void should_name_the_sink_of_a_duplicate() => Validate(spec => spec.Sinks.Configurations.Add(new() { Name = "Reporting", Kind = StorageKinds.PostgreSql, ConnectionSecretRef = new() { Name = "other" } })).Single().Sink.ShouldEqual("Reporting");
}
#endif
