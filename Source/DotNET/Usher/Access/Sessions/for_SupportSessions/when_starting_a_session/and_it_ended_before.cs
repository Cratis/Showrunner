// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Monads;
using Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.given;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.when_starting_a_session;

public class and_it_ended_before : support_sessions
{
    Result<SupportSessionStart, SupportSessionRefusal> _result;

    async Task Because()
    {
        await Granted();
        await _sessions.Start(_eventStore, Request, IssueEvents.Support);
        await _sessions.End(_eventStore, _issueId, SessionId);
        _result = await _sessions.Start(_eventStore, Request, IssueEvents.Support);
        await ReadEvents();
    }

    [Fact] void should_refuse_it() => Refusal().ShouldEqual(SupportSessionRefusal.SessionEnded);
    [Fact] void should_not_resume_it() => _events[^1].Content.ShouldEqual(new SupportSessionEnded(SessionId));

    SupportSessionRefusal Refusal()
    {
        _result.TryGetError(out var refusal);
        return refusal;
    }
}
#endif
