// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_a_managed_vault_is_ready : a_production_reconciliation
{
    const string Token = "e2e-vault-token";

    protected override ProductionSpec CreateSpec()
    {
        var spec = ProductionSamples.External();
        spec.Vault.Mode = ProductionModes.Managed;
        return spec;
    }

    void Establish()
    {
        _cluster.StatefulSetsReady = true;
        _cluster.SecretValues["apps/ledger-vault-chronicle/token"] = Token;
    }

    Task Because() => Reconcile();

    [Fact] void should_be_ready() => _phase.ShouldEqual(ProductionPhase.Ready);
    [Fact] void should_report_the_vault_ready() => Condition(ProductionConditions.VaultReady).Reason.ShouldEqual("Ready");
    [Fact] void should_store_keys_in_vault() => Environment("Cratis__Chronicle__Compliance__Encryption__Storage__Type")!.Value.ShouldEqual("vault");
    [Fact] void should_tell_chronicle_where_the_vault_is() => Environment("Cratis__Chronicle__Compliance__Encryption__Storage__ConnectionDetails")!.Value.ShouldEqual("http://ledger-vault.apps.svc.cluster.local:8200");
    [Fact] void should_give_chronicle_its_token_from_the_secret() => Environment("VAULT_TOKEN")!.ValueFrom.SecretKeyRef.Name.ShouldEqual("ledger-vault-chronicle");
    [Fact] void should_read_the_token_from_its_key() => Environment("VAULT_TOKEN")!.ValueFrom.SecretKeyRef.Key.ShouldEqual("token");
    [Fact] void should_keep_the_token_out_of_the_statefulsets() => JsonSerializer.Serialize(Chronicle).Contains(Token, StringComparison.Ordinal).ShouldBeFalse();
    [Fact] void should_keep_the_token_out_of_the_status() => StatusAsJson.Contains(Token, StringComparison.Ordinal).ShouldBeFalse();
}
#endif
