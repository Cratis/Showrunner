// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Monads;
using Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.given;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.when_checking_a_grant;

public class and_it_is_live : support_sessions
{
    Result<LiveGrant, SupportSessionRefusal> _result;

    async Task Because()
    {
        await Granted();
        _result = await _sessions.Check(_eventStore, Request);
        await ReadEvents();
    }

    [Fact] void should_be_live() => _result.IsSuccess.ShouldBeTrue();
    [Fact] void should_answer_until_the_grant_ends() => Grant().ValidUntil.ShouldEqual(ValidUntil);
    [Fact] void should_represent_the_reporter_as_they_shared_who_they_are() => Grant().Reporter.ShouldEqual(Reporters.Reporter);
    [Fact] void should_record_that_the_session_was_permitted() => _events.Select(_ => _.Content).OfType<SupportSessionPermitted>().ShouldContainOnly(new SupportSessionPermitted(SessionId));

    LiveGrant Grant()
    {
        _result.TryGetResult(out var grant);
        return grant;
    }
}
#endif
