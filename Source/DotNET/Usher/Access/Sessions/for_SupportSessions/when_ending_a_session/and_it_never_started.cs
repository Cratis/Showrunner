// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.given;

namespace Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.when_ending_a_session;

public class and_it_never_started : support_sessions
{
    SupportSessionEnd _result;

    async Task Because()
    {
        await Granted();
        _result = await _sessions.End(_eventStore, _issueId, SessionId);
        await ReadEvents();
    }

    [Fact] void should_say_it_never_started() => _result.ShouldEqual(SupportSessionEnd.NeverStarted);
    [Fact] void should_record_nothing() => _events.ShouldNotContain(_ => _.Content is SupportSessionEnded);
}
#endif
