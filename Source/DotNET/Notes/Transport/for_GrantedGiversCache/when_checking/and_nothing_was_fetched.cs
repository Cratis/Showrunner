// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Transport.given;

namespace Cratis.Showrunner.Notes.Transport.for_GrantedGiversCache.when_checking;

/// <summary>
/// No list was ever fetched, not even a refused one: nobody is granted, because the application fails closed.
/// </summary>
public class and_nothing_was_fetched : a_granted_givers_cache
{
    [Fact] void should_not_have_a_list() => _cache.HasList.ShouldBeFalse();
    [Fact] void should_grant_nobody() => _cache.IsGranted(TheGiver).ShouldBeFalse();
}
#endif
