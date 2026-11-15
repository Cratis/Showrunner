// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler.given;

namespace Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler;

public class when_the_production_does_not_exist : an_understudy_reconciliation
{
    void Establish() => _cluster.Production = null;

    async Task Because() => await Reconcile();

    [Fact] void should_block() => _phase.ShouldEqual(UnderstudyPhase.Blocked);
    [Fact] void should_say_the_production_was_not_found() => Condition(UnderstudyConditions.ProductionFound).Reason.ShouldEqual("ProductionNotFound");
    [Fact] void should_name_it() => Condition(UnderstudyConditions.ProductionFound).Message.ShouldContain("ledger");
    [Fact] void should_schedule_nothing() => _cluster.Applied.ShouldBeEmpty();
}
#endif
