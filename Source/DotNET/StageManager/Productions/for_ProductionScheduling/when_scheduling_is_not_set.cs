// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;
using k8s.Models;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionScheduling;

public class when_scheduling_is_not_set : Specification
{
    V1StatefulSet _chronicle;

    void Because()
    {
        var production = ProductionSamples.Production(ProductionSamples.External());
        _chronicle = ChronicleWorkload.StatefulSet(production, ChronicleKernelConfiguration.Build(production.Spec, ProductionSamples.Names));
    }

    [Fact] void should_not_select_nodes() => _chronicle.Spec.Template.Spec.NodeSelector.ShouldBeNull();
    [Fact] void should_not_tolerate_taints() => _chronicle.Spec.Template.Spec.Tolerations.ShouldBeNull();
    [Fact] void should_not_set_resources() => _chronicle.Spec.Template.Spec.Containers[0].Resources.ShouldBeNull();
}
#endif
