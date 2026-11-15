// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_a_managed_vault_is_not_ready_yet : a_production_reconciliation
{
    V1StatefulSet _vault;

    protected override ProductionSpec CreateSpec()
    {
        var spec = ProductionSamples.External();
        spec.Vault.Mode = ProductionModes.Managed;
        return spec;
    }

    async Task Because()
    {
        await Reconcile();
        _vault = _cluster.AppliedNamed<V1StatefulSet>("ledger-vault")!;
    }

    [Fact] void should_be_progressing() => _phase.ShouldEqual(ProductionPhase.Progressing);
    [Fact] void should_run_one_node() => _vault.Spec.Replicas.ShouldEqual(1);
    [Fact] void should_run_the_pinned_image() => _vault.Spec.Template.Spec.Containers[0].Image.ShouldEqual(ProductionManagedVault.DefaultImage);
    [Fact] void should_keep_its_data_on_a_volume() => _vault.Spec.VolumeClaimTemplates.Single().Spec.Resources.Requests["storage"].ToString().ShouldEqual("1Gi");
    [Fact] void should_run_a_helper_next_to_the_node() => _vault.Spec.Template.Spec.Containers.Select(container => container.Name).ShouldContainOnly(["vault", "bootstrap"]);
    [Fact] void should_give_the_helper_a_service_account_of_its_own() => _vault.Spec.Template.Spec.ServiceAccountName.ShouldEqual("ledger-vault-bootstrap");
    [Fact] void should_apply_the_configuration() => _cluster.AppliedNamed<V1ConfigMap>("ledger-vault-config")!.Data.ContainsKey("vault.hcl").ShouldBeTrue();
    [Fact] void should_apply_a_role_that_only_reads_and_creates_secrets() => _cluster.AppliedNamed<V1Role>("ledger-vault-bootstrap")!.Rules.Single().Verbs.ShouldContainOnly(["get", "create"]);
    [Fact] void should_bind_the_role_to_the_service_account() => _cluster.AppliedNamed<V1RoleBinding>("ledger-vault-bootstrap")!.Subjects.Single().Name.ShouldEqual("ledger-vault-bootstrap");
    [Fact] void should_apply_a_service_for_chronicle() => _cluster.AppliedNamed<V1Service>("ledger-vault")!.Spec.Ports.Single().Port.ShouldEqual(8200);
    [Fact] void should_apply_a_headless_service_for_the_nodes() => _cluster.AppliedNamed<V1Service>("ledger-vault-nodes")!.Spec.ClusterIP.ShouldEqual("None");
    [Fact] void should_say_the_vault_is_not_ready() => Condition(ProductionConditions.VaultReady).Reason.ShouldEqual("NotReady");
    [Fact] void should_not_apply_chronicle() => Chronicle.ShouldBeNull();
    [Fact] void should_wait_for_the_dependencies() => Condition(ProductionConditions.ChronicleReady).Reason.ShouldEqual("WaitingForDependencies");
    [Fact] void should_report_the_vault_it_runs_with() => _production.Status.Vault.ShouldEqual("Managed/Development");
}
#endif
