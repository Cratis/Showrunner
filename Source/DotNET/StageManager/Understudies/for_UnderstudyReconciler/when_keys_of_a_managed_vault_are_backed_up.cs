// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler.given;
using Cratis.Showrunner.StageManager.Understudies.given;
using Cratis.Showrunner.StageManager.Productions;
using Cratis.Showrunner.StageManager.Understudies.Jobs;

namespace Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler;

public class when_keys_of_a_managed_vault_are_backed_up : an_understudy_reconciliation
{
    protected override bool Vault => true;

    protected override UnderstudySpec CreateSpec()
    {
        var spec = UnderstudySamples.Spec();
        spec.Keys = UnderstudySamples.Keys();
        return spec;
    }

    V1Container Container => KeysCronJob!.Spec.JobTemplate.Spec.Template.Spec.Containers.Single();

    void Establish() => _cluster.Production.Spec.Vault = new() { Mode = "Managed" };

    async Task Because() => await Reconcile();

    [Fact] void should_schedule_the_key_backups() => KeysCronJob.ShouldNotBeNull();
    [Fact] void should_say_the_key_backups_are_ready() => Condition(UnderstudyConditions.KeyBackupsReady).Status.ShouldEqual("True");
    [Fact] void should_tell_the_job_the_address_of_the_vault_the_operator_runs() => UnderstudyJobConfiguration.FromJson(Container.Env.Single(variable => variable.Name == UnderstudyJobEnvironment.Configuration).Value).VaultAddress.ShouldEqual(ProductionNames.For(_cluster.Production).VaultAddress);
}
#endif
