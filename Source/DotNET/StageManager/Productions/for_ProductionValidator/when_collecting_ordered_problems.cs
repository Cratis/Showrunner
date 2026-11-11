// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionValidator;

public class when_collecting_ordered_problems : Specification
{
    ProductionSpec _spec;
    IReadOnlyList<ProductionProblem> _problems;

    void Establish()
    {
        _spec = ProductionSamples.WithSinks();
        _spec.Version = " ";
        _spec.Replicas = 0;
        _spec.Database.Mode = "Hosted";
        _spec.Graph.Mode = "Hosted";
        _spec.Graph.Edition = "Aura";
        _spec.Sinks.Configurations[0] = new() { Name = " ", Kind = "Oracle" };
        _spec.Sinks.Configurations[1].ConnectionSecretRef = null;
        _spec.Sinks.Configurations[1].GraphRef = "managed";
        _spec.Sinks.Configurations.Add(new() { Name = "People", Kind = StorageKinds.Neo4j });
        _spec.Sinks.ReadModels = "people";
        _spec.Sinks.Graphs = "warehouse";
    }

    void Because() => _problems = ProductionValidator.ValidateEach(_spec);

    [Fact]
    void should_keep_message_order_and_sink_attribution() => _problems.ShouldEqual<ProductionProblem>(new[]
    {
        new ProductionProblem("version is required: the tag of the cratis/chronicle image to run."),
        new ProductionProblem("replicas must be at least 1, not 0."),
        new ProductionProblem("database.mode 'Hosted' is unknown. Use one of Managed, External."),
        new ProductionProblem("graph.mode 'Hosted' is unknown. Use one of None, Managed, External."),
        new ProductionProblem("The sink name ' ' may only hold letters and digits, separated by single hyphens or underscores.", " "),
        new ProductionProblem("The sink 'warehouse' is SqlServer: graphRef is for Neo4j sinks only.", "warehouse"),
        new ProductionProblem("The sink 'warehouse' is SqlServer and needs a connectionSecretRef.", "warehouse"),
        new ProductionProblem("The sink 'people' uses the managed graph, but graph.mode is Hosted.", "people"),
        new ProductionProblem("The sink 'People' is configured twice.", "People"),
        new ProductionProblem("sinks.readModels names the sink 'people', which is Neo4j and only stores graphs.", "people"),
        new ProductionProblem("sinks.graphs names the sink 'warehouse', which is SqlServer and cannot store graphs.", "warehouse")
    });
}
#endif
