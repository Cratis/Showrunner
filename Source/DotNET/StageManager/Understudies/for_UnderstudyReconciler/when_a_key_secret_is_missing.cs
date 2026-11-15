// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler.given;
using Cratis.Showrunner.StageManager.Understudies.given;
using Cratis.Showrunner.StageManager.Understudies.Jobs;
using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler;

public class when_a_key_secret_is_missing : an_understudy_reconciliation
{
    protected override bool Vault => true;

    protected override UnderstudySpec CreateSpec()
    {
        var spec = UnderstudySamples.Spec();
        spec.Keys = UnderstudySamples.Keys();
        return spec;
    }

    void Establish() => _cluster.SecretValues.Remove("apps/vault-reader/token");

    async Task Because() => await Reconcile();

    [Fact] void should_not_schedule_the_key_backups() => KeysCronJob.ShouldBeNull();
    [Fact] void should_say_why() => Condition(UnderstudyConditions.KeyBackupsReady).Reason.ShouldEqual("SecretNotFound");
}
#endif
