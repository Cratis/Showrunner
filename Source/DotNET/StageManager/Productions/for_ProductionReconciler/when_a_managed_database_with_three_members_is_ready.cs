// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_a_managed_database_with_three_members_is_ready : a_production_reconciliation
{
    PerconaServerMongoDB _database;

    protected override ProductionSpec CreateSpec()
    {
        var spec = ProductionSamples.Managed();
        spec.Database.Members = 3;
        spec.Database.OneMemberPerNode = false;
        spec.Database.StorageSize = "20Gi";
        spec.Database.StorageClassName = "fast";
        spec.Database.NodeSelector["pool"] = "databases";
        return spec;
    }

    void Establish()
    {
        _cluster.MongoDBState = "ready";
        _cluster.StatefulSetsReady = true;
        _cluster.SecretValues["apps/ledger-mongodb-users/MONGODB_DATABASE_ADMIN_USER"] = "databaseAdmin";
        _cluster.SecretValues["apps/ledger-mongodb-users/MONGODB_DATABASE_ADMIN_PASSWORD"] = "p@ss/word";
    }

    async Task Because()
    {
        await Reconcile();
        _database = _cluster.AppliedNamed<PerconaServerMongoDB>("ledger-mongodb")!;
    }

    [Fact] void should_be_ready() => _phase.ShouldEqual(ProductionPhase.Ready);
    [Fact] void should_run_three_members() => _database.Spec["replsets"]![0]!["size"]!.GetValue<int>().ShouldEqual(3);
    [Fact] void should_not_need_the_development_flag() => _database.Spec["unsafeFlags"]!.AsObject().ContainsKey("replsetSize").ShouldBeFalse();
    [Fact] void should_let_members_share_a_node_when_asked() => _database.Spec["replsets"]![0]!["affinity"]!["antiAffinityTopologyKey"]!.ToString().ShouldEqual("none");
    [Fact] void should_size_the_volumes() => _database.Spec["replsets"]![0]!["volumeSpec"]!["persistentVolumeClaim"]!["resources"]!["requests"]!["storage"]!.ToString().ShouldEqual("20Gi");
    [Fact] void should_use_the_storage_class() => _database.Spec["replsets"]![0]!["volumeSpec"]!["persistentVolumeClaim"]!["storageClassName"]!.ToString().ShouldEqual("fast");
    [Fact] void should_place_the_members() => _database.Spec["replsets"]![0]!["nodeSelector"]!["pool"]!.ToString().ShouldEqual("databases");
    [Fact] void should_compose_the_connection_from_the_percona_user() =>
        _cluster.AppliedSecretValue("ledger-mongodb-connection", "connectionString").ShouldEqual("mongodb://databaseAdmin:p%40ss%2Fword@ledger-mongodb-rs0.apps.svc.cluster.local:27017/?replicaSet=rs0&authSource=admin");
    [Fact] void should_mount_the_composed_connection_into_chronicle() => Chronicle!.Spec.Template.Spec.Volumes.Single(volume => volume.Name == "chronicle-connections").Projected.Sources.Single().Secret.Name.ShouldEqual("ledger-mongodb-connection");
    [Fact] void should_report_the_database_ready() => Condition(ProductionConditions.DatabaseReady).Status.ShouldEqual("True");
}
#endif
