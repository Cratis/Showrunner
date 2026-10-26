// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Monads;
using Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.given;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.when_checking_a_grant;

/// <summary>
/// A subject is unique only within its identity provider: an actor with the reporter's subject at another provider is
/// somebody else, and may act as the reporter like any other support person.
/// </summary>
public class and_the_actor_has_the_reporters_subject_at_another_provider : support_sessions
{
    Result<LiveGrant, SupportSessionRefusal> _result;

    async Task Because()
    {
        await Granted();
        _result = await _sessions.Check(_eventStore, Request with { Actor = Reporters.SameSubjectAtAnotherProvider });
    }

    [Fact] void should_be_live() => _result.IsSuccess.ShouldBeTrue();
}
#endif
