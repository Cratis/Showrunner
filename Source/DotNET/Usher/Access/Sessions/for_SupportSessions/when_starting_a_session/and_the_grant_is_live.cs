// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Monads;
using Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.given;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.when_starting_a_session;

public class and_the_grant_is_live : support_sessions
{
    Result<SupportSessionStart, SupportSessionRefusal> _result;

    async Task Because()
    {
        await Granted();
        _result = await _sessions.Start(_eventStore, Request, IssueEvents.Support);
        await ReadEvents();
    }

    [Fact] void should_start_it() => Start().ShouldEqual(SupportSessionStart.Started);
    [Fact] void should_record_who_signed_in() => _events[^1].Content.ShouldEqual(new SupportSignedInAsReporter(SessionId, IssueEvents.Support));

    SupportSessionStart Start()
    {
        _result.TryGetResult(out var start);
        return start;
    }
}
#endif
