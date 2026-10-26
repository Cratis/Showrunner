// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Monads;
using Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.given;

namespace Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.when_checking_a_session;

public class and_it_was_only_permitted : support_sessions
{
    Result<LiveGrant, SupportSessionRefusal> _result;

    async Task Because()
    {
        await Granted();
        await Happened(new SupportSessionPermitted(SessionId));
        _result = await _sessions.CheckSession(_eventStore, Request);
    }

    [Fact] void should_not_be_live() => _result.IsSuccess.ShouldBeFalse();
    [Fact] void should_say_it_never_started() => Refusal().ShouldEqual(SupportSessionRefusal.SessionNotStarted);

    SupportSessionRefusal Refusal()
    {
        _result.TryGetError(out var refusal);
        return refusal;
    }
}
#endif
