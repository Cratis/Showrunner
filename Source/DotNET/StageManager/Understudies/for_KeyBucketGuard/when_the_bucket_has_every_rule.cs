// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.for_KeyBucketGuard.given;
using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies.for_KeyBucketGuard;

public class when_the_bucket_has_every_rule : a_keys_bucket
{
    async Task Because() => await Check();

    [Fact] void should_hold() => _check.Holds.ShouldBeTrue();
    [Fact] void should_say_the_rules_hold() => _check.Reason.ShouldEqual(BucketRulesCheck.RulesHold);
    [Fact] void should_apply_the_expiry_rule() => _storage.ExpiryDays.ShouldEqual(ExpiryDays);
}
#endif
