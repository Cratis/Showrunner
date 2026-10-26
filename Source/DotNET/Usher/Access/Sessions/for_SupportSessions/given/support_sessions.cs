// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Collections.Immutable;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.Testing.Events;
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Contracts.Access;
using Cratis.Showrunner.Usher.Contracts.Identity;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Identity;
using Cratis.Showrunner.Usher.Identity.ShareIdentity;

namespace Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.given;

public class support_sessions : Specification
{
    protected static readonly DateTimeOffset Now = new(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);
    protected static readonly ValidUntil ValidUntil = new(Now.AddHours(1));
    protected static readonly UserIdentity Actor = new("entra", "support-person");
    protected static readonly SupportSessionId SessionId = "a-session";

    protected EventStoreForTesting _eventStore;
    protected ScenarioTime _time;
    protected SupportSessions _sessions;
    protected IssueId _issueId;
    protected IImmutableList<AppendedEvent> _events;

    protected SupportSessionRequest Request => new(_issueId, SessionId, Reporters.Subject, Actor);

    protected SupportSessionRequest RequestFor(SupportSessionId sessionId) => Request with { SessionId = sessionId };

    void Establish()
    {
        _eventStore = new();
        _time = new(Now);
        _sessions = new(_time, new RecordingLogger<SupportSessions>());
        _issueId = IssueId.New();
    }

    void Destroy() => _eventStore.Dispose();

    protected async Task Happened(params object[] events)
    {
        await _eventStore.EventLog.Append(_issueId, IssueEvents.Described(Reporters.Identity));
        foreach (var @event in events)
        {
            await _eventStore.EventLog.Append(_issueId, @event);
        }
    }

    protected Task Granted() => Happened(new ReporterSharedIdentity(Reporters.Reporter), new ReporterGrantedAccess(ValidUntil));

    protected async Task ReadEvents() =>
        _events = await _eventStore.EventLog.GetFromSequenceNumber(EventSequenceNumber.First, _issueId);
}
#endif
