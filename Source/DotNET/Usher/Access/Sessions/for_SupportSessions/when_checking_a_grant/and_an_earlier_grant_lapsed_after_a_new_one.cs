// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Monads;
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Access.Lapsing;
using Cratis.Showrunner.Usher.Access.RevokeAccess;
using Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.given;
using Cratis.Showrunner.Usher.Contracts.Access;

namespace Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.when_checking_a_grant;

public class and_an_earlier_grant_lapsed_after_a_new_one : support_sessions
{
    static readonly ValidUntil _earlier = new(Now.AddMinutes(-1));
    Result<LiveGrant, SupportSessionRefusal> _result;

    async Task Because()
    {
        await Granted();
        await Happened(new ReporterRevokedAccess(), new ReporterGrantedAccess(_earlier), new ReporterGrantedAccess(ValidUntil), new AccessGrantLapsed(_earlier));
        _result = await _sessions.Check(_eventStore, Request);
    }

    [Fact] void should_be_live() => _result.IsSuccess.ShouldBeTrue();
}
#endif
