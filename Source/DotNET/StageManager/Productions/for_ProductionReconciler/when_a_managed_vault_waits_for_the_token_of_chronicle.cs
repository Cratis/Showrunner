// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_a_managed_vault_waits_for_the_token_of_chronicle : a_production_reconciliation
{
    protected override ProductionSpec CreateSpec()
    {
        var spec = ProductionSamples.External();
        spec.Vault.Mode = ProductionModes.Managed;
        return spec;
    }

    void Establish() => _cluster.StatefulSetsReady = true;

    Task Because() => Reconcile();

    [Fact] void should_be_progressing() => _phase.ShouldEqual(ProductionPhase.Progressing);
    [Fact] void should_say_it_waits_for_the_token() => Condition(ProductionConditions.VaultReady).Reason.ShouldEqual("WaitingForToken");
    [Fact] void should_not_apply_chronicle() => Chronicle.ShouldBeNull();
}
#endif
