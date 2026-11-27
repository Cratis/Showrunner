// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text.Json.Nodes;
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_the_managed_database_is_tuned : a_production_reconciliation
{
    PerconaServerMongoDB _database;

    protected override ProductionSpec CreateSpec()
    {
        var spec = ProductionSamples.Managed();
        spec.Database.MemoryLimit = "1536Mi";
        spec.Database.MemoryRequest = "1536Mi";
        spec.Database.CpuRequest = "250m";
        spec.Database.CpuLimit = "2";
        spec.Database.WiredTigerCacheSizeRatio = 0.39;
        spec.Database.SetParameters["wiredTigerConcurrentReadTransactions"] = "64";
        spec.Database.SetParameters["logLevel"] = "warn";
        return spec;
    }

    async Task Because()
    {
        await Reconcile();
        _database = _cluster.AppliedNamed<PerconaServerMongoDB>("ledger-mongodb")!;
    }

    JsonNode ReplicaSet => _database.Spec["replsets"]![0]!;

    [Fact] void should_size_the_cache() => ReplicaSet["storage"]!["wiredTiger"]!["engineConfig"]!["cacheSizeRatio"]!.GetValue<double>().ShouldEqual(0.39);
    [Fact] void should_use_the_wired_tiger_engine() => ReplicaSet["storage"]!["engine"]!.GetValue<string>().ShouldEqual("wiredTiger");
    [Fact] void should_request_the_memory() => ReplicaSet["resources"]!["requests"]!["memory"]!.GetValue<string>().ShouldEqual("1536Mi");
    [Fact] void should_request_the_cpu() => ReplicaSet["resources"]!["requests"]!["cpu"]!.GetValue<string>().ShouldEqual("250m");
    [Fact] void should_limit_the_cpu() => ReplicaSet["resources"]!["limits"]!["cpu"]!.GetValue<string>().ShouldEqual("2");
    [Fact] void should_limit_the_memory() => ReplicaSet["resources"]!["limits"]!["memory"]!.GetValue<string>().ShouldEqual("1536Mi");
    [Fact] void should_release_allocator_memory_and_set_the_given_parameters() =>
        ReplicaSet["configuration"]!.GetValue<string>().ShouldEqual("setParameter:\n  logLevel: \"warn\"\n  tcmallocReleaseRate: 16777216\n  wiredTigerConcurrentReadTransactions: 64\n");
}
#endif
