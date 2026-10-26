// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.Reactors;
using Cratis.Showrunner.Usher.Access.Lapsing;
using Cratis.Showrunner.Usher.Access.RevokeAccess;

namespace Cratis.Showrunner.Usher.Access.Sessions;

/// <summary>
/// Ends the sessions in which support signed in as the reporter when the grant they ran on ends: the reporter revoked
/// it, or it lapsed.
/// </summary>
/// <remarks>
/// AuthProxy stops a session at its next grant check, which answers not live as soon as the grant ended, and then
/// reports the end itself. This records the end on Usher's side without waiting for that, so the reporter sees the
/// session end with the grant. Both report the same end, and it is recorded once. A lapse of an earlier grant after a
/// new one was given ends nothing, and a session that started after the grant ended is not this reactor's to end.
/// </remarks>
/// <param name="client">The <see cref="IChronicleClient"/> giving the event store of the namespace the event is in.</param>
/// <param name="sessions">The <see cref="SupportSessions"/>.</param>
[Reactor(Id)]
public class SessionsEndWithGrant(IChronicleClient client, SupportSessions sessions) : IReactor
{
    /// <summary>
    /// The stable identifier of the reactor.
    /// </summary>
    public const string Id = "Cratis.Showrunner.Usher.Access.Sessions.SessionsEndWithGrant";

    /// <summary>
    /// Ends the open sessions when the reporter revoked the grant.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="context">The event context.</param>
    /// <returns>An awaitable task.</returns>
    public Task AccessRevoked(ReporterRevokedAccess @event, EventContext context) => EndSessions(context);

    /// <summary>
    /// Ends the open sessions when the grant lapsed.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="context">The event context.</param>
    /// <returns>An awaitable task.</returns>
    public Task GrantLapsed(AccessGrantLapsed @event, EventContext context) => EndSessions(context);

    async Task EndSessions(EventContext context)
    {
        var eventStore = await client.GetEventStore(context.EventStore, context.Namespace);
        await sessions.EndSessionsOfEndedGrant(eventStore, new IssueId(Guid.Parse(context.EventSourceId.Value)), context.SequenceNumber);
    }
}
