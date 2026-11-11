// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_the_managed_database_reports_no_state : a_production_reconciliation
{
    protected override ProductionSpec CreateSpec() => ProductionSamples.Managed();

    Task Because() => Reconcile();

    [Fact] void should_be_progressing() => _phase.ShouldEqual(ProductionPhase.Progressing);
    [Fact] void should_say_the_database_is_not_ready() => Condition(ProductionConditions.DatabaseReady).Reason.ShouldEqual("NotReady");
    [Fact] void should_say_it_reports_no_state_yet() => Condition(ProductionConditions.DatabaseReady).Message.ShouldContain("not reporting a state yet");
    [Fact] void should_make_chronicle_wait_even_with_the_certificate() => Condition(ProductionConditions.ChronicleReady).Reason.ShouldEqual("WaitingForDependencies");
    [Fact] void should_still_write_the_connection_secret() => Condition(ProductionConditions.ConnectionAvailable).Reason.ShouldEqual("Written");
}
#endif
