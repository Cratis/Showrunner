// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.RevokeAccess;
using Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.when_the_grant_ends.given;

namespace Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.when_the_grant_ends;

public class and_authproxy_also_reports_the_end : a_grant_with_sessions
{
    SupportSessionEnd _reported;

    async Task Because()
    {
        await GrantEndedWith(new ReporterRevokedAccess());

        // Two AuthProxy replicas notice the revocation at their next grant check.
        _reported = await _sessions.End(_eventStore, _issueId, Open);
        await _sessions.End(_eventStore, _issueId, Open);
        await ReadEvents();
    }

    [Fact] void should_know_it_already_ended() => _reported.ShouldEqual(SupportSessionEnd.AlreadyEnded);
    [Fact] void should_record_the_end_once() => EndsOf(Open).Count().ShouldEqual(1);
}
#endif
