// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_a_managed_vault_runs_in_the_production_tier : a_production_reconciliation
{
    V1StatefulSet _vault;
    string _configuration;
    string _script;

    protected override ProductionSpec CreateSpec()
    {
        var spec = ProductionSamples.External();
        spec.Vault.Mode = ProductionModes.Managed;
        spec.Vault.Managed.Tier = ProductionManagedVault.ProductionTier;
        spec.Vault.Managed.StorageClassName = "fast";
        return spec;
    }

    async Task Because()
    {
        await Reconcile();
        _vault = _cluster.AppliedNamed<V1StatefulSet>("ledger-vault")!;
        var configMap = _cluster.AppliedNamed<V1ConfigMap>("ledger-vault-config")!;
        _configuration = configMap.Data["vault.hcl"];
        _script = configMap.Data["bootstrap.sh"];
    }

    [Fact] void should_run_three_nodes() => _vault.Spec.Replicas.ShouldEqual(3);
    [Fact] void should_start_the_nodes_together() => _vault.Spec.PodManagementPolicy.ShouldEqual("Parallel");
    [Fact] void should_use_raft_storage() => _configuration.ShouldContain("storage \"raft\"");
    [Fact] void should_not_use_file_storage() => _configuration.Contains("storage \"file\"", StringComparison.Ordinal).ShouldBeFalse();
    [Fact] void should_join_every_node_to_the_first() => _configuration.Split("retry_join").Length.ShouldEqual(4);
    [Fact] void should_name_the_nodes_through_the_headless_service() => _configuration.ShouldContain("http://ledger-vault-2.ledger-vault-nodes.apps.svc.cluster.local:8200");
    [Fact] void should_use_the_storage_class() => _vault.Spec.VolumeClaimTemplates.Single().Spec.StorageClassName.ShouldEqual("fast");
    [Fact] void should_tell_the_helper_it_is_the_production_tier() => _vault.Spec.Template.Spec.Containers.Single(container => container.Name == "bootstrap").Env.Single(variable => variable.Name == "TIER").Value.ShouldEqual("Production");
    [Fact] void should_restrict_the_token_of_chronicle_to_its_own_key_paths() => _script.ShouldContain("path \"secret/data/k1/*\"");
    [Fact] void should_let_chronicle_read_the_keys_of_the_earlier_layout() => _script.ShouldContain("path \"secret/data/*\"        { capabilities = [\"read\"] }");
    [Fact] void should_let_chronicle_list_the_keys_of_the_earlier_layout() => _script.ShouldContain("path \"secret/metadata/*\"    { capabilities = [\"read\", \"list\", \"delete\"] }");
    [Fact] void should_wait_for_an_uninitialized_vault_to_answer() => _script.ShouldContain(".initialized | tostring");
    [Fact] void should_not_hand_chronicle_the_root_policy() => _script.Contains("\"policies\":[\"root\"]", StringComparison.Ordinal).ShouldBeFalse();
    [Fact] void should_keep_the_token_alive() => _script.ShouldContain("renew-self");
    [Fact] void should_roll_the_nodes_when_the_configuration_changes() => _vault.Spec.Template.Metadata.Annotations.ContainsKey(ManagedVault.ConfigRevisionAnnotation).ShouldBeTrue();
    [Fact] void should_report_the_tier() => _production.Status.Vault.ShouldEqual("Managed/Production");
}
#endif
