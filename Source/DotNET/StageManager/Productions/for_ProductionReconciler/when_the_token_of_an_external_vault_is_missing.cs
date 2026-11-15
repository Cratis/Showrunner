// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_the_token_of_an_external_vault_is_missing : a_production_reconciliation
{
    protected override ProductionSpec CreateSpec()
    {
        var spec = ProductionSamples.External();
        spec.Vault.Mode = ProductionModes.External;
        spec.Vault.Address = "https://vault.example.com:8200";
        spec.Vault.TokenSecretRef = new() { Name = "vault-token" };
        return spec;
    }

    Task Because() => Reconcile();

    [Fact] void should_be_blocked() => _phase.ShouldEqual(ProductionPhase.Blocked);
    [Fact] void should_apply_nothing() => _cluster.Applied.ShouldBeEmpty();
    [Fact] void should_name_the_secret() => Condition(ProductionConditions.VaultReady).Message.ShouldContain("vault-token");
}
#endif
