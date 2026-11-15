// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using Cratis.Showrunner.Notes.Transport.given;

namespace Cratis.Showrunner.Notes.Transport.for_GrantedGiversCache.when_checking;

/// <summary>
/// The collector cannot be reached, but it was confirmed a moment ago: the last list stands.
/// </summary>
public class and_the_refresh_fails_within_the_staleness : a_granted_givers_cache
{
    Exception _failed;

    async Task Because()
    {
        Serve(4, TheGiver);
        await _cache.Refresh();
        _collector.FailWith = new CollectorRequestFailed(HttpStatusCode.ServiceUnavailable);
        _time.Advance(Staleness);
        _failed = await Record.ExceptionAsync(() => _cache.Refresh());
    }

    [Fact] void should_report_the_failure() => _failed.ShouldBeOfExactType<CollectorRequestFailed>();
    [Fact] void should_keep_the_last_list() => _cache.IsGranted(TheGiver).ShouldBeTrue();
}
#endif
