// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.given;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.when_ending_a_session;

public class and_the_end_is_reported_twice : support_sessions
{
    SupportSessionEnd _first;
    SupportSessionEnd _second;

    async Task Because()
    {
        await Granted();
        await _sessions.Start(_eventStore, Request, IssueEvents.Support);
        _first = await _sessions.End(_eventStore, _issueId, SessionId);
        _second = await _sessions.End(_eventStore, _issueId, SessionId);
        await ReadEvents();
    }

    [Fact] void should_end_it_the_first_time() => _first.ShouldEqual(SupportSessionEnd.Ended);
    [Fact] void should_know_it_already_ended_the_second_time() => _second.ShouldEqual(SupportSessionEnd.AlreadyEnded);
    [Fact] void should_record_the_end_once() => _events.Count(_ => _.Content is SupportSessionEnded).ShouldEqual(1);
}
#endif
