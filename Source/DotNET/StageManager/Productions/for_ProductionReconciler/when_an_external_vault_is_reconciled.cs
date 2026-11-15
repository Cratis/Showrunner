// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_an_external_vault_is_reconciled : a_production_reconciliation
{
    protected override ProductionSpec CreateSpec()
    {
        var spec = ProductionSamples.External();
        spec.Vault.Mode = ProductionModes.External;
        spec.Vault.Address = "https://vault.example.com:8200";
        spec.Vault.TokenSecretRef = new() { Name = "vault-token", Key = "value" };
        return spec;
    }

    void Establish()
    {
        _cluster.StatefulSetsReady = true;
        _cluster.SecretValues["apps/vault-token/value"] = "e2e-vault-token";
    }

    Task Because() => Reconcile();

    [Fact] void should_be_ready() => _phase.ShouldEqual(ProductionPhase.Ready);
    [Fact] void should_create_no_vault_resources() => _cluster.AppliedNamed<V1StatefulSet>("ledger-vault").ShouldBeNull();
    [Fact] void should_say_the_vault_is_external() => Condition(ProductionConditions.VaultReady).Reason.ShouldEqual("External");
    [Fact] void should_tell_chronicle_where_the_vault_is() => Environment("Cratis__Chronicle__Compliance__Encryption__Storage__ConnectionDetails")!.Value.ShouldEqual("https://vault.example.com:8200");
    [Fact] void should_give_chronicle_the_token_from_the_named_key() => Environment("VAULT_TOKEN")!.ValueFrom.SecretKeyRef.Key.ShouldEqual("value");
}
#endif
