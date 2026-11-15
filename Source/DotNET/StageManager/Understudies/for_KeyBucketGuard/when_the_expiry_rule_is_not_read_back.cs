// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.for_KeyBucketGuard.given;
using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies.for_KeyBucketGuard;

public class when_the_expiry_rule_is_not_read_back : a_keys_bucket
{
    async Task Because()
    {
        _storage.ReadBackExpiry = false;
        await Check();
    }

    [Fact] void should_refuse() => _check.Holds.ShouldBeFalse();
    [Fact] void should_say_the_expiry_is_missing() => _check.Reason.ShouldEqual(BucketRulesCheck.ExpiryMissing);
}
#endif
