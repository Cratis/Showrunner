// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.given;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.when_the_grant_ends.given;

public class a_grant_with_sessions : support_sessions
{
    protected static readonly SupportSessionId Open = "open-session";
    protected static readonly SupportSessionId Ended = "ended-session";

    async Task Establish()
    {
        await Granted();
        await _sessions.Start(_eventStore, RequestFor(Ended), IssueEvents.Support);
        await _sessions.End(_eventStore, _issueId, Ended);
        await _sessions.Start(_eventStore, RequestFor(Open), IssueEvents.Agent);
    }

    protected async Task GrantEndedWith(object @event)
    {
        await Happened(@event);
        await ReadEvents();
        await _sessions.EndSessionsOfEndedGrant(_eventStore, _issueId, _events[^1].Context.SequenceNumber);
        await ReadEvents();
    }

    protected IEnumerable<SupportSessionId> EndsOf(SupportSessionId sessionId) =>
        _events.Select(_ => _.Content).OfType<SupportSessionEnded>().Where(_ => _.SessionId == sessionId).Select(_ => _.SessionId);
}
#endif
