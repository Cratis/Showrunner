// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using Cratis.Showrunner.Notes.Transport.given;

namespace Cratis.Showrunner.Notes.Transport.for_GrantedGiversCache.when_checking;

/// <summary>
/// The collector cannot be reached for longer than the list may be kept: nobody is granted until a refresh succeeds, and
/// then the list is as the collector says.
/// </summary>
public class and_the_refresh_fails_beyond_the_staleness : a_granted_givers_cache
{
    bool _grantedAfterFailing;
    bool _grantedAfterRecovering;

    async Task Because()
    {
        Serve(4, TheGiver);
        await _cache.Refresh();
        _collector.FailWith = new CollectorRequestFailed(HttpStatusCode.ServiceUnavailable);
        _time.Advance(Staleness + TimeSpan.FromSeconds(1));
        await Record.ExceptionAsync(() => _cache.Refresh());
        _grantedAfterFailing = _cache.IsGranted(TheGiver);
        _collector.FailWith = null;
        await _cache.Refresh();
        _grantedAfterRecovering = _cache.IsGranted(TheGiver);
    }

    [Fact] void should_grant_nobody_while_it_cannot_be_confirmed() => _grantedAfterFailing.ShouldBeFalse();
    [Fact] void should_grant_again_when_the_collector_confirms_the_list() => _grantedAfterRecovering.ShouldBeTrue();
}
#endif
