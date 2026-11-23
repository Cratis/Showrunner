// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionScheduling;

public class when_a_managed_database_is_scheduled : Specification
{
    System.Text.Json.Nodes.JsonNode _replicaSet;

    void Because()
    {
        var spec = ProductionSamples.Managed();
        spec.Scheduling.NodeSelector["workload"] = "data";
        spec.Scheduling.Tolerations.Add(new() { Key = "workload", Value = "data", Effect = "NoSchedule" });
        _replicaSet = ManagedMongoDB.Create(ProductionSamples.Production(spec)).Spec["replsets"]![0]!;
    }

    [Fact] void should_select_the_nodes() => _replicaSet["nodeSelector"]!["workload"]!.ToString().ShouldEqual("data");
    [Fact] void should_tolerate_the_taint() => _replicaSet["tolerations"]![0]!["effect"]!.ToString().ShouldEqual("NoSchedule");
}
#endif
