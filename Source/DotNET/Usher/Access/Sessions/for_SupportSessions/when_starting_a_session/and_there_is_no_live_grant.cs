// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Monads;
using Cratis.Showrunner.Usher.Access.RevokeAccess;
using Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.given;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.when_starting_a_session;

public class and_there_is_no_live_grant : support_sessions
{
    Result<SupportSessionStart, SupportSessionRefusal> _result;

    async Task Because()
    {
        await Granted();
        await Happened(new ReporterRevokedAccess());
        _result = await _sessions.Start(_eventStore, Request, IssueEvents.Support);
        await ReadEvents();
    }

    [Fact] void should_refuse_it() => Refusal().ShouldEqual(SupportSessionRefusal.NoLiveGrant);
    [Fact] void should_not_record_a_session() => _events.ShouldNotContain(_ => _.Content is SupportSignedInAsReporter);

    SupportSessionRefusal Refusal()
    {
        _result.TryGetError(out var refusal);
        return refusal;
    }
}
#endif
