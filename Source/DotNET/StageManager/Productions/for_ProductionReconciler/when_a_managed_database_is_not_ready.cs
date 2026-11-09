// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_a_managed_database_is_not_ready : a_production_reconciliation
{
    PerconaServerMongoDB _database;

    protected override ProductionSpec CreateSpec() => ProductionSamples.Managed();

    void Establish() => _cluster.MongoDBState = "initializing";

    async Task Because()
    {
        await Reconcile();
        _database = _cluster.AppliedNamed<PerconaServerMongoDB>("ledger-mongodb")!;
    }

    string Value(params string[] path) => path.Aggregate((System.Text.Json.Nodes.JsonNode)_database.Spec, (node, segment) => node[segment]!).ToString();

    [Fact] void should_be_progressing() => _phase.ShouldEqual(ProductionPhase.Progressing);
    [Fact] void should_run_one_member() => _database.Spec["replsets"]![0]!["size"]!.GetValue<int>().ShouldEqual(1);
    [Fact] void should_accept_one_member_as_development() => Value("unsafeFlags", "replsetSize").ShouldEqual("true");
    [Fact] void should_pin_the_image() => Value("image").ShouldEqual("percona/percona-server-mongodb:7.0.14-8");
    [Fact] void should_never_upgrade_automatically() => Value("upgradeOptions", "apply").ShouldEqual("disabled");
    [Fact] void should_disable_tls() => Value("tls", "mode").ShouldEqual("disabled");
    [Fact] void should_limit_memory_to_8gi() => _database.Spec["replsets"]![0]!["resources"]!["limits"]!["memory"]!.ToString().ShouldEqual("8Gi");
    [Fact] void should_run_one_member_per_node() => _database.Spec["replsets"]![0]!["affinity"]!["antiAffinityTopologyKey"]!.ToString().ShouldEqual("kubernetes.io/hostname");
    [Fact] void should_keep_its_users_in_a_secret_of_its_own() => Value("secrets", "users").ShouldEqual("ledger-mongodb-users");
    [Fact] void should_not_run_chronicle_yet() => Chronicle.ShouldBeNull();
    [Fact] void should_say_the_database_is_not_ready() => Condition(ProductionConditions.DatabaseReady).Message.ShouldContain("initializing");
    [Fact] void should_report_the_database_it_runs_with() => _production.Status.Database.ShouldEqual("MongoDB/Managed");
}
#endif
