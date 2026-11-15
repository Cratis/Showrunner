// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.given;
using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies.for_KeyBucketGuard.given;

public abstract class a_keys_bucket : Specification
{
    protected const int ExpiryDays = 35;

    protected InMemoryObjectStorage _storage;
    protected BucketRulesCheck _check;

    void Establish() => _storage = new InMemoryObjectStorage();

    protected async Task Check() => _check = await KeyBucketGuard.EnsureAsync(_storage, ExpiryDays, CancellationToken.None);
}
#endif
