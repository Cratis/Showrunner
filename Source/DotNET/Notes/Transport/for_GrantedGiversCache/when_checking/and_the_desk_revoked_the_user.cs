// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Transport.given;

namespace Cratis.Showrunner.Notes.Transport.for_GrantedGiversCache.when_checking;

/// <summary>
/// The desk revokes a user the application had as granted: the next refresh, one poll later, takes the right away, and the
/// application names the version it had.
/// </summary>
public class and_the_desk_revoked_the_user : a_granted_givers_cache
{
    bool _grantedBefore;

    async Task Because()
    {
        Serve(4, TheGiver, SomeoneElse);
        await _cache.Refresh();
        _grantedBefore = _cache.IsGranted(TheGiver);
        Serve(5, SomeoneElse);
        _time.Advance(TimeSpan.FromSeconds(30));
        await _cache.Refresh();
    }

    [Fact] void should_have_granted_the_user_before() => _grantedBefore.ShouldBeTrue();
    [Fact] void should_refuse_the_user_after_one_refresh() => _cache.IsGranted(TheGiver).ShouldBeFalse();
    [Fact] void should_keep_whoever_is_still_granted() => _cache.IsGranted(SomeoneElse).ShouldBeTrue();
    [Fact] void should_name_the_version_it_had() => _collector.AccessAskedWith.ShouldContainOnly([null, 4UL]);
}
#endif
