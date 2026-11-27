// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text.Json.Nodes;
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_the_managed_database_is_not_tuned : a_production_reconciliation
{
    PerconaServerMongoDB _database;

    protected override ProductionSpec CreateSpec() => ProductionSamples.Managed();

    async Task Because()
    {
        await Reconcile();
        _database = _cluster.AppliedNamed<PerconaServerMongoDB>("ledger-mongodb")!;
    }

    JsonNode ReplicaSet => _database.Spec["replsets"]![0]!;

    [Fact] void should_leave_the_cache_to_the_operator() => (ReplicaSet["storage"] is null).ShouldBeTrue();
    [Fact] void should_request_nothing() => (ReplicaSet["resources"]!["requests"] is null).ShouldBeTrue();
    [Fact] void should_not_limit_the_cpu() => (ReplicaSet["resources"]!["limits"]!["cpu"] is null).ShouldBeTrue();
    [Fact] void should_release_allocator_memory_by_default() =>
        ReplicaSet["configuration"]!.GetValue<string>().ShouldEqual("setParameter:\n  tcmallocReleaseRate: 16777216\n");
    [Fact] void should_not_run_exporters() => _cluster.AppliedOf<V1Deployment>().Any().ShouldBeFalse();
}
#endif
