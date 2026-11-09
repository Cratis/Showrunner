// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Performances.for_PerformanceReconciler;

public class when_the_application_reads_a_graph_the_production_does_not_have : given_a_performance_on_a_production
{
    void Establish()
    {
        _performance.Spec.Application.Graph = true;
        _cluster.Productions["apps/ledger"] = ProductionSamples.External();
    }

    Task Because() => Reconcile();

    [Fact] void should_be_blocked() => _phase.ShouldEqual(PerformancePhase.Blocked);
    [Fact] void should_not_have_its_graph_bound() => Condition("GraphBound").Status.ShouldEqual("False");
    [Fact] void should_give_the_reason() => Condition("GraphBound").Reason.ShouldEqual("ProductionHasNoGraph");
}
#endif
