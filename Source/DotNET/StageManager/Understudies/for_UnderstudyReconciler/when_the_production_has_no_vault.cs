// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler.given;
using Cratis.Showrunner.StageManager.Understudies.given;
using Cratis.Showrunner.StageManager.Understudies.Jobs;
using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler;

public class when_the_production_has_no_vault : an_understudy_reconciliation
{
    protected override UnderstudySpec CreateSpec()
    {
        var spec = UnderstudySamples.Spec();
        spec.Keys = UnderstudySamples.Keys();
        return spec;
    }

    async Task Because() => await Reconcile();

    [Fact] void should_not_schedule_key_backups() => KeysCronJob.ShouldBeNull();
    [Fact] void should_say_the_keys_travel_with_the_events() => Condition(UnderstudyConditions.KeyBackupsReady).Reason.ShouldEqual("NoVault");
    [Fact] void should_not_open_the_keys_bucket() => _storages.Asked.Select(settings => settings.Bucket).ShouldNotContain("ledger-keys");
    [Fact] void should_not_be_degraded_for_it() => _phase.ShouldEqual(UnderstudyPhase.Scheduled);
}
#endif
