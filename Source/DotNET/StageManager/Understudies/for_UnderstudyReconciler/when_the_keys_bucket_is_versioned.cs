// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler.given;
using Cratis.Showrunner.StageManager.Understudies.given;
using Cratis.Showrunner.StageManager.Understudies.Jobs;
using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler;

public class when_the_keys_bucket_is_versioned : an_understudy_reconciliation
{
    protected override bool Vault => true;

    protected override UnderstudySpec CreateSpec()
    {
        var spec = UnderstudySamples.Spec();
        spec.Keys = UnderstudySamples.Keys();
        return spec;
    }

    void Establish() => Keys.Versioned = true;

    async Task Because() => await Reconcile();

    [Fact] void should_not_schedule_the_key_backups() => KeysCronJob.ShouldBeNull();
    [Fact] void should_say_the_key_backups_are_not_ready() => Condition(UnderstudyConditions.KeyBackupsReady).Status.ShouldEqual("False");
    [Fact] void should_say_why() => Condition(UnderstudyConditions.KeyBackupsReady).Reason.ShouldEqual(BucketRulesCheck.Versioned);
    [Fact] void should_keep_backing_up_the_databases() => BackupCronJob.ShouldNotBeNull();
    [Fact] void should_be_degraded() => _phase.ShouldEqual(UnderstudyPhase.Degraded);
}
#endif
