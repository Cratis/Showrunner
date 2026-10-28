// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Monads;
using Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.given;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.when_checking_a_session;

public class and_it_started : support_sessions
{
    Result<LiveGrant, SupportSessionRefusal> _result;

    async Task Because()
    {
        await Granted();
        await Happened(new SupportSignedInAsReporter(SessionId, IssueEvents.Support, IssueEvents.SupportPerson));
        _result = await _sessions.CheckSession(_eventStore, Request);
    }

    [Fact] void should_be_live() => _result.IsSuccess.ShouldBeTrue();
}
#endif
