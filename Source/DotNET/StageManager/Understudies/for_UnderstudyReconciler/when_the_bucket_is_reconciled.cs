// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler.given;

namespace Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler;

public class when_the_bucket_is_reconciled : an_understudy_reconciliation
{
    async Task Because() => await Reconcile();

    [Fact] void should_use_the_settings_of_the_spec() => (_storages.Asked[0].Endpoint, _storages.Asked[0].Bucket, _storages.Asked[0].PathStyle).ShouldEqual(("https://objects.example.com", "ledger-backups", true));
    [Fact] void should_use_the_credentials_of_the_secret() => (_storages.Asked[0].AccessKey, _storages.Asked[0].SecretKey).ShouldEqual(("access-key-value", "secret-key-value"));
    [Fact] void should_apply_the_maximum_age_as_an_expiry_rule() => Data.ExpiryDays.ShouldEqual(35);
    [Fact] void should_say_the_lifecycle_is_enforced() => Condition(UnderstudyConditions.LifecycleEnforced).Status.ShouldEqual("True");
}
#endif
