// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.for_KeyBucketGuard.given;
using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies.for_KeyBucketGuard;

public class when_the_bucket_is_versioned : a_keys_bucket
{
    async Task Because()
    {
        _storage.Versioned = true;
        await Check();
    }

    [Fact] void should_refuse() => _check.Holds.ShouldBeFalse();
    [Fact] void should_say_it_is_versioned() => _check.Reason.ShouldEqual(BucketRulesCheck.Versioned);
    [Fact] void should_say_what_to_do() => _check.Message.ShouldContain("versioning");
    [Fact] void should_not_touch_the_bucket() => _storage.ExpiryDays.ShouldBeNull();
}
#endif
