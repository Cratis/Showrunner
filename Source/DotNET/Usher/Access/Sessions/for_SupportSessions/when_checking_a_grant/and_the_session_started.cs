// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Monads;
using Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.given;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.when_checking_a_grant;

public class and_the_session_started : support_sessions
{
    Result<LiveGrant, SupportSessionRefusal> _first;
    Result<LiveGrant, SupportSessionRefusal> _again;

    async Task Because()
    {
        await Granted();
        _first = await _sessions.Check(_eventStore, Request);
        await _sessions.Start(_eventStore, Request, IssueEvents.Support);
        _again = await _sessions.Check(_eventStore, Request);
        await ReadEvents();
    }

    [Fact] void should_be_live_before_it_starts() => _first.IsSuccess.ShouldBeTrue();
    [Fact] void should_be_live_again_while_it_lasts() => _again.IsSuccess.ShouldBeTrue();
    [Fact] void should_record_the_permit_only_once() => _events.Count(_ => _.Content is SupportSessionPermitted).ShouldEqual(1);
}
#endif
