// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_PerformanceReconciler;

public class when_the_application_names_egress_rules : a_reconciliation
{
    protected override Performance CreatePerformance()
    {
        var performance = PerformanceSamples.Performance();
        performance.Spec.Application.Egress = [new() { Ports = [587] }];
        return performance;
    }

    void Establish() => _cluster.DeploymentsReady = true;

    Task Because() => Reconcile();

    [Fact] void should_apply_the_egress_policy() => _cluster.AppliedOf<V1NetworkPolicy>().Single().Metadata.Name.ShouldEqual("crm-egress");
    [Fact] void should_delete_nothing() => _cluster.Deleted.ShouldBeEmpty();
}
#endif
