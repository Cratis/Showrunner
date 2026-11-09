// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_the_graph_is_managed : a_production_reconciliation
{
    string _password;
    V1StatefulSet _neo4j;

    protected override ProductionSpec CreateSpec()
    {
        var spec = ProductionSamples.External();
        spec.Graph = new() { Mode = ProductionModes.Managed, StorageSize = "5Gi", StorageClassName = "fast" };
        return spec;
    }

    async Task Because()
    {
        await Reconcile();
        _password = _cluster.AppliedSecretValue("ledger-neo4j", "password")!;
        await Reconcile();
        _neo4j = _cluster.AppliedNamed<V1StatefulSet>("ledger-neo4j")!;
    }

    [Fact] void should_generate_a_password() => string.IsNullOrEmpty(_password).ShouldBeFalse();
    [Fact] void should_keep_the_password() => _cluster.AppliedSecretValue("ledger-neo4j", "password").ShouldEqual(_password);
    [Fact] void should_give_neo4j_its_credentials_from_the_secret() => _neo4j.Spec.Template.Spec.Containers.Single().Env.Single().ValueFrom.SecretKeyRef.Name.ShouldEqual("ledger-neo4j");
    [Fact] void should_run_a_pinned_community_image() => _neo4j.Spec.Template.Spec.Containers.Single().Image.ShouldEqual("neo4j:5.26.0-community");
    [Fact] void should_keep_its_data_on_a_volume() => _neo4j.Spec.VolumeClaimTemplates.Single().Spec.Resources.Requests["storage"].ToString().ShouldEqual("5Gi");
    [Fact] void should_use_the_storage_class() => _neo4j.Spec.VolumeClaimTemplates.Single().Spec.StorageClassName.ShouldEqual("fast");
    [Fact] void should_serve_bolt() => _cluster.AppliedNamed<V1Service>("ledger-neo4j")!.Spec.Ports.Any(port => port.Port == 7687).ShouldBeTrue();
    [Fact] void should_tell_the_kernel_where_the_server_is() => Environment("Cratis__Chronicle__Neo4j__ServerFile")!.Value.ShouldEqual("/run/secrets/chronicle/Cratis__Chronicle__Neo4j__Server");
    [Fact] void should_run_the_kernel_on_one_database() => Environment("Cratis__Chronicle__Neo4j__DatabaseMode")!.Value.ShouldEqual("Single");
    [Fact] void should_store_the_bolt_address_in_the_secret() => _cluster.AppliedSecretValue("ledger-neo4j", "server").ShouldEqual("bolt://ledger-neo4j.apps.svc.cluster.local:7687");
    [Fact] void should_keep_the_password_out_of_the_spec_and_status() => (JsonSerializer.Serialize(_production.Spec) + StatusAsJson).Contains(_password, StringComparison.Ordinal).ShouldBeFalse();
    [Fact] void should_keep_the_password_out_of_the_statefulsets() => (JsonSerializer.Serialize(_neo4j) + JsonSerializer.Serialize(Chronicle)).Contains(_password, StringComparison.Ordinal).ShouldBeFalse();
    [Fact] void should_say_the_graph_is_not_ready_yet() => Condition(ProductionConditions.GraphReady).Status.ShouldEqual("False");
    [Fact] void should_report_the_graph_it_runs_with() => _production.Status.Graph.ShouldEqual("Managed/Community");
}
#endif
