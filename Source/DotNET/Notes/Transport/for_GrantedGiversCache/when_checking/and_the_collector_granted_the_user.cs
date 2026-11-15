// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Transport.given;

namespace Cratis.Showrunner.Notes.Transport.for_GrantedGiversCache.when_checking;

/// <summary>
/// The collector names the user: they are granted, and someone it does not name is not.
/// </summary>
public class and_the_collector_granted_the_user : a_granted_givers_cache
{
    async Task Because()
    {
        Serve(4, TheGiver);
        await _cache.Refresh();
    }

    [Fact] void should_have_a_list() => _cache.HasList.ShouldBeTrue();
    [Fact] void should_grant_the_user() => _cache.IsGranted(TheGiver).ShouldBeTrue();
    [Fact] void should_not_grant_someone_the_collector_does_not_name() => _cache.IsGranted(SomeoneElse).ShouldBeFalse();
    [Fact] void should_ask_without_a_version_the_first_time() => _collector.AccessAskedWith.ShouldContainOnly([null]);
}
#endif
