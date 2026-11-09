// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_chronicle_is_ready : a_production_reconciliation
{
    void Establish() => _cluster.StatefulSetsReady = true;

    Task Because() => Reconcile();

    [Fact] void should_be_ready() => _phase.ShouldEqual(ProductionPhase.Ready);
    [Fact] void should_report_chronicle_ready() => Condition(ProductionConditions.ChronicleReady).Status.ShouldEqual("True");
    [Fact] void should_report_the_graph_ready_without_one() => Condition(ProductionConditions.GraphReady).Reason.ShouldEqual("NoGraph");
}
#endif
