// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.for_KeyBucketGuard.given;
using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies.for_KeyBucketGuard;

public class when_the_provider_cannot_apply_an_expiry_rule : a_keys_bucket
{
    async Task Because()
    {
        _storage.CanEnforceExpiry = false;
        await Check();
    }

    [Fact] void should_refuse() => _check.Holds.ShouldBeFalse();
    [Fact] void should_say_it_cannot_prove_the_rules() => _check.Reason.ShouldEqual(BucketRulesCheck.NotProvable);
}
#endif
