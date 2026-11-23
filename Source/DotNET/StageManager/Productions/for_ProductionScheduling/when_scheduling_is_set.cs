// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;
using k8s.Models;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionScheduling;

public class when_scheduling_is_set : Specification
{
    V1StatefulSet _chronicle;
    V1StatefulSet _vault;

    void Because()
    {
        var spec = ProductionSamples.Managed();
        spec.Vault.Mode = ProductionModes.Managed;
        spec.Scheduling.NodeSelector["workload"] = "data";
        spec.Scheduling.Tolerations.Add(new() { Key = "workload", Value = "data", Effect = "NoSchedule" });
        spec.Resources.Requests["memory"] = "1Gi";
        var production = ProductionSamples.Production(spec);
        _chronicle = ChronicleWorkload.StatefulSet(production, ChronicleKernelConfiguration.Build(production.Spec, ProductionSamples.Names));
        _vault = ManagedVault.StatefulSet(production);
    }

    [Fact] void should_select_the_nodes_for_chronicle() => _chronicle.Spec.Template.Spec.NodeSelector["workload"].ShouldEqual("data");
    [Fact] void should_tolerate_the_taint_for_chronicle() => _chronicle.Spec.Template.Spec.Tolerations.Single().Effect.ShouldEqual("NoSchedule");
    [Fact] void should_reserve_what_is_requested_for_chronicle() => _chronicle.Spec.Template.Spec.Containers[0].Resources.Requests["memory"].ToString().ShouldEqual("1Gi");
    [Fact] void should_select_the_nodes_for_vault() => _vault.Spec.Template.Spec.NodeSelector["workload"].ShouldEqual("data");
    [Fact] void should_tolerate_the_taint_for_vault() => _vault.Spec.Template.Spec.Tolerations.Single().Key.ShouldEqual("workload");
}
#endif
