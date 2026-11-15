// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler.given;

namespace Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler;

public class when_the_bucket_cannot_be_reached : an_understudy_reconciliation
{
    void Establish() => Data.Unreachable = true;

    async Task Because() => await Reconcile();

    [Fact] void should_say_the_storage_is_not_ready() => Condition(UnderstudyConditions.StorageReady).Status.ShouldEqual("False");
    [Fact] void should_say_it_is_unreachable() => Condition(UnderstudyConditions.StorageReady).Reason.ShouldEqual("Unreachable");
    [Fact] void should_not_echo_what_the_provider_said() => Condition(UnderstudyConditions.StorageReady).Message.ShouldEqual("The bucket could not be read (HttpRequestException).");
    [Fact] void should_still_schedule_the_backups() => BackupCronJob.ShouldNotBeNull();
    [Fact] void should_be_degraded() => _phase.ShouldEqual(UnderstudyPhase.Degraded);
}
#endif
