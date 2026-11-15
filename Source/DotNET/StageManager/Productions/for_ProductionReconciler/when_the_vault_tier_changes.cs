// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_the_vault_tier_changes : a_production_reconciliation
{
    protected override ProductionSpec CreateSpec()
    {
        var spec = ProductionSamples.External();
        spec.Vault.Mode = ProductionModes.Managed;
        spec.Vault.Managed.Tier = ProductionManagedVault.ProductionTier;
        return spec;
    }

    void Establish() => _production.Status = new() { Database = "MongoDB/External", Graph = "None/Community", Vault = "Managed/Development" };

    Task Because() => Reconcile();

    [Fact] void should_be_blocked() => _phase.ShouldEqual(ProductionPhase.Blocked);
    [Fact] void should_apply_nothing() => _cluster.Applied.ShouldBeEmpty();
    [Fact] void should_name_the_tier() => Condition(ProductionConditions.VaultModeUnchanged).Message.ShouldContain("vault.managed.tier from Development to Production");
}
#endif
