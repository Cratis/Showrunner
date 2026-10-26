// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Monads;
using Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.given;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.when_starting_a_session;

public class and_it_already_started : support_sessions
{
    Result<SupportSessionStart, SupportSessionRefusal> _result;

    async Task Because()
    {
        await Granted();
        await _sessions.Start(_eventStore, Request, IssueEvents.Support);
        _result = await _sessions.Start(_eventStore, Request, IssueEvents.Support);
        await ReadEvents();
    }

    [Fact] void should_acknowledge_it() => Start().ShouldEqual(SupportSessionStart.AlreadyStarted);
    [Fact] void should_record_it_once() => _events.Count(_ => _.Content is SupportSignedInAsReporter).ShouldEqual(1);

    SupportSessionStart Start()
    {
        _result.TryGetResult(out var start);
        return start;
    }
}
#endif
