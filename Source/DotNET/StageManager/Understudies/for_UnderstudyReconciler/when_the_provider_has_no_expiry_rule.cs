// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler.given;

namespace Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler;

public class when_the_provider_has_no_expiry_rule : an_understudy_reconciliation
{
    void Establish() => Data.CanEnforceExpiry = false;

    async Task Because() => await Reconcile();

    [Fact] void should_say_the_lifecycle_is_not_enforced() => Condition(UnderstudyConditions.LifecycleEnforced).Status.ShouldEqual("False");
    [Fact] void should_say_why() => Condition(UnderstudyConditions.LifecycleEnforced).Reason.ShouldEqual("NotSupported");
    [Fact] void should_still_schedule_the_backups() => BackupCronJob.ShouldNotBeNull();
    [Fact] void should_not_block() => _phase.ShouldEqual(UnderstudyPhase.Scheduled);
}
#endif
