// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_the_managed_database_has_metrics : a_production_reconciliation
{
    List<V1Deployment> _exporters;

    protected override ProductionSpec CreateSpec()
    {
        var spec = ProductionSamples.Managed();
        spec.Database.Members = 3;
        spec.Database.Metrics.Enabled = true;
        spec.Database.Metrics.NodeSelector["workload"] = "compute";
        return spec;
    }

    void Establish()
    {
        _cluster.MongoDBState = "ready";
        _cluster.SecretValues["apps/ledger-mongodb-users/MONGODB_DATABASE_ADMIN_USER"] = "databaseAdmin";
        _cluster.SecretValues["apps/ledger-mongodb-users/MONGODB_DATABASE_ADMIN_PASSWORD"] = "password";
    }

    async Task Because()
    {
        await Reconcile();
        _exporters = [.. _cluster.AppliedOf<V1Deployment>().Where(deployment => deployment.Metadata.Name.Contains("-exporter-", StringComparison.Ordinal))];
    }

    V1Container Exporter(int member) => _exporters.Single(deployment => deployment.Metadata.Name == $"ledger-mongodb-exporter-{member}").Spec.Template.Spec.Containers.Single();

    [Fact] void should_run_one_exporter_per_member() => _exporters.Count.ShouldEqual(3);
    [Fact] void should_name_them_after_the_members() => _exporters.Select(deployment => deployment.Metadata.Name).Order().ShouldContainOnly("ledger-mongodb-exporter-0", "ledger-mongodb-exporter-1", "ledger-mongodb-exporter-2");
    [Fact] void should_select_each_member_separately() => _exporters.Select(deployment => deployment.Spec.Selector.MatchLabels[MongoDBExporter.MemberLabel]).Order().ShouldContainOnly("0", "1", "2");
    [Fact] void should_connect_directly_to_the_member() => Exporter(1).Env.Single(variable => variable.Name == "MONGODB_URI").Value.ShouldEqual("mongodb://ledger-mongodb-rs0-1.ledger-mongodb-rs0.apps.svc.cluster.local:27017/admin?authSource=admin");
    [Fact] void should_read_the_monitor_user_from_the_users_secret() => Exporter(0).Env.Single(variable => variable.Name == "MONGODB_USER").ValueFrom.SecretKeyRef.Key.ShouldEqual("MONGODB_CLUSTER_MONITOR_USER");
    [Fact] void should_read_the_monitor_password_from_the_users_secret() => Exporter(0).Env.Single(variable => variable.Name == "MONGODB_PASSWORD").ValueFrom.SecretKeyRef.Name.ShouldEqual("ledger-mongodb-users");
    [Fact] void should_run_the_pinned_exporter() => Exporter(0).Image.ShouldEqual(ProductionDatabaseMetrics.DefaultImage);
    [Fact] void should_run_only_the_cheap_collectors() => Exporter(0).Args.ShouldContain("--collector.diagnosticdata", "--collector.replicasetstatus", "--no-collector.exporter-metrics");
    [Fact] void should_annotate_the_pods_for_prometheus() => _exporters[0].Spec.Template.Metadata.Annotations["prometheus.io/port"].ShouldEqual("9216");
    [Fact] void should_place_the_exporters() => _exporters[0].Spec.Template.Spec.NodeSelector["workload"].ShouldEqual("compute");
}
#endif
