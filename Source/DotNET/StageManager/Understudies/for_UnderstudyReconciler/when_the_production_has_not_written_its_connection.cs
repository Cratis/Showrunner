// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler.given;

namespace Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler;

public class when_the_production_has_not_written_its_connection : an_understudy_reconciliation
{
    void Establish() => _cluster.Production!.Status.ConnectionSecretName = string.Empty;

    async Task Because() => await Reconcile();

    [Fact] void should_block() => _phase.ShouldEqual(UnderstudyPhase.Blocked);
    [Fact] void should_say_the_connection_is_not_available() => Condition(UnderstudyConditions.ProductionFound).Reason.ShouldEqual("ConnectionNotAvailable");
    [Fact] void should_schedule_nothing() => _cluster.Applied.ShouldBeEmpty();
}
#endif
