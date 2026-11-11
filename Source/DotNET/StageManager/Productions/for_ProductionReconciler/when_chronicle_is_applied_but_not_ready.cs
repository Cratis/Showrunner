// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_chronicle_is_applied_but_not_ready : a_production_reconciliation
{
    protected override ProductionSpec CreateSpec()
    {
        var spec = ProductionSamples.External();
        spec.Replicas = 3;
        return spec;
    }

    Task Because() => Reconcile();

    [Fact] void should_be_progressing() => _phase.ShouldEqual(ProductionPhase.Progressing);
    [Fact] void should_run_chronicle() => Chronicle.ShouldNotBeNull();
    [Fact] void should_say_chronicle_is_not_ready() => Condition(ProductionConditions.ChronicleReady).Reason.ShouldEqual("NotReady");
    [Fact] void should_count_the_ready_replicas() => Condition(ProductionConditions.ChronicleReady).Message.ShouldEqual($"0 of 3 replica(s) of Chronicle {_production.Spec.Version} are ready.");
    [Fact] void should_write_the_connection_secret() => Condition(ProductionConditions.ConnectionAvailable).Reason.ShouldEqual("Written");
    [Fact] void should_name_the_connection_secret() => _production.Status.ConnectionSecretName.ShouldEqual("ledger-chronicle-connection");
}
#endif
