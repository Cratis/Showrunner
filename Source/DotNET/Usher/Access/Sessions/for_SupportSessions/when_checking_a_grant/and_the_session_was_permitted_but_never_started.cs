// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.when_checking_a_grant;

public class and_the_session_was_permitted_but_never_started : a_refusal
{
    async Task Because()
    {
        await Granted();
        await _sessions.Check(_eventStore, Request);
        _result = await _sessions.Check(_eventStore, Request);
        await ReadEvents();
    }

    [Fact] void should_not_be_live() => _result.IsSuccess.ShouldBeFalse();
    [Fact] void should_say_why() => Refusal().ShouldEqual(SupportSessionRefusal.SessionNotStarted);
    [Fact] void should_record_the_permit_only_once() => _events.Count(_ => _.Content is SupportSessionPermitted).ShouldEqual(1);
}
#endif
