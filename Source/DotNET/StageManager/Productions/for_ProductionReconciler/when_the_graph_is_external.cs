// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_the_graph_is_external : a_production_reconciliation
{
    protected override ProductionSpec CreateSpec()
    {
        var spec = ProductionSamples.External();
        spec.Graph = new() { Mode = ProductionModes.External, Edition = GraphEditions.Enterprise, CredentialsSecretName = "shared-graph" };
        return spec;
    }

    void Establish()
    {
        _cluster.SecretValues["apps/shared-graph/server"] = "bolt://shared:7687";
        _cluster.SecretValues["apps/shared-graph/password"] = "e2e-graph-password";
        _cluster.StatefulSetsReady = true;
    }

    Task Because() => Reconcile();

    [Fact] void should_be_ready() => _phase.ShouldEqual(ProductionPhase.Ready);
    [Fact] void should_create_no_neo4j() => _cluster.Applied.Exists(resource => resource.Metadata.Name == "ledger-neo4j").ShouldBeFalse();
    [Fact] void should_mount_the_referenced_secret() => Chronicle!.Spec.Template.Spec.Volumes.Single(volume => volume.Name == "chronicle-connections").Projected.Sources.Any(source => source.Secret.Name == "shared-graph").ShouldBeTrue();
    [Fact] void should_use_a_database_per_namespace() => Environment("Cratis__Chronicle__Neo4j__DatabaseMode")!.Value.ShouldEqual("PerNamespace");
    [Fact] void should_prefix_the_databases() => Environment("Cratis__Chronicle__Neo4j__DatabasePrefix")!.Value.ShouldEqual("ledger");
    [Fact] void should_report_the_graph_ready() => Condition(ProductionConditions.GraphReady).Reason.ShouldEqual("External");
    [Fact] void should_keep_the_password_out_of_everything_it_wrote() => (JsonSerializer.Serialize(Chronicle) + StatusAsJson).Contains("e2e-graph-password", StringComparison.Ordinal).ShouldBeFalse();
}
#endif
