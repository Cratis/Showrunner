// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_the_vault_mode_changes : a_production_reconciliation
{
    protected override ProductionSpec CreateSpec()
    {
        var spec = ProductionSamples.External();
        spec.Vault.Mode = ProductionModes.Managed;
        return spec;
    }

    void Establish() => _production.Status = new() { Database = "MongoDB/External", Graph = "None/Community", Vault = "None" };

    Task Because() => Reconcile();

    [Fact] void should_be_blocked() => _phase.ShouldEqual(ProductionPhase.Blocked);
    [Fact] void should_apply_nothing() => _cluster.Applied.ShouldBeEmpty();
    [Fact] void should_refuse_the_change() => Condition(ProductionConditions.VaultModeUnchanged).Reason.ShouldEqual("ModeChangeRefused");
    [Fact] void should_name_the_mode() => Condition(ProductionConditions.VaultModeUnchanged).Message.ShouldContain("vault.mode from None to Managed");
}
#endif
