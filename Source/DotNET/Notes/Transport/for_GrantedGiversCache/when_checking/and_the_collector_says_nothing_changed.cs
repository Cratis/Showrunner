// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Transport.given;

namespace Cratis.Showrunner.Notes.Transport.for_GrantedGiversCache.when_checking;

/// <summary>
/// The collector answers that the list is still current: the application keeps it, and the answer counts as confirming it, so
/// it does not go stale while the desk decides nothing.
/// </summary>
public class and_the_collector_says_nothing_changed : a_granted_givers_cache
{
    async Task Because()
    {
        Serve(4, TheGiver);
        await _cache.Refresh();
        _time.Advance(Staleness);
        await _cache.Refresh();
        _time.Advance(Staleness);
    }

    [Fact] void should_still_grant_the_user() => _cache.IsGranted(TheGiver).ShouldBeTrue();
}
#endif
