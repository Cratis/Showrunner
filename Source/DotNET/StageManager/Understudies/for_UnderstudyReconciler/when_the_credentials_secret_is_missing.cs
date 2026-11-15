// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler.given;

namespace Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler;

public class when_the_credentials_secret_is_missing : an_understudy_reconciliation
{
    void Establish() => _cluster.SecretValues.Clear();

    async Task Because() => await Reconcile();

    [Fact] void should_block() => _phase.ShouldEqual(UnderstudyPhase.Blocked);
    [Fact] void should_say_the_secret_was_not_found() => Condition(UnderstudyConditions.StorageReady).Reason.ShouldEqual("SecretNotFound");
    [Fact] void should_name_the_secret_and_its_keys() => Condition(UnderstudyConditions.StorageReady).Message.ShouldContain("backup-credentials");
    [Fact] void should_schedule_nothing() => _cluster.Applied.ShouldBeEmpty();
    [Fact] void should_not_reach_for_the_bucket() => _storages.Asked.ShouldBeEmpty();
}
#endif
