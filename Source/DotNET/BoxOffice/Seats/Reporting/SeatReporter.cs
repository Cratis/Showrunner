// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.Reactors;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.BoxOffice.Seats.Reporting;

/// <summary>
/// Reports to the operator who holds the tenant's seats: <c>SeatTaken</c> and <c>SeatReleased</c>, in the outbox under the
/// operator's organization.
/// </summary>
/// <param name="client">The <see cref="IChronicleClient"/>.</param>
/// <param name="logger">The <see cref="ILogger{T}"/>.</param>
[Reactor(Id)]
[OnceOnly]
public class SeatReporter(IChronicleClient client, ILogger<SeatReporter> logger) : IReactor
{
    /// <summary>
    /// The identity of the reactor.
    /// </summary>
    public const string Id = "Cratis.Showrunner.BoxOffice.Seats.Reporting.SeatReporter";

    /// <summary>
    /// Maps a seat fact to what the operator is told.
    /// </summary>
    /// <param name="fact">The private fact.</param>
    /// <param name="tenant">The tenant.</param>
    /// <returns>The public event.</returns>
    /// <exception cref="NotASeatFact">The fact is not one the operator is told about.</exception>
    public static object PublicEventFor(object fact, TenantId tenant) => fact switch
    {
        SeatAssigned e => new SeatTaken(tenant, e.Holder),
        SeatUnassigned e => new SeatReleased(tenant, e.Holder),
        _ => throw new NotASeatFact(fact.GetType())
    };

    /// <summary>
    /// Reports a seat that was assigned.
    /// </summary>
    /// <param name="event">The fact.</param>
    /// <param name="context">The <see cref="EventContext"/>.</param>
    /// <param name="delivery">The <see cref="ReactorDelivery"/>.</param>
    /// <returns>An awaitable task.</returns>
    public Task Assigned(SeatAssigned @event, EventContext context, ReactorDelivery delivery) => Report(@event, context, delivery);

    /// <summary>
    /// Reports a seat that was released.
    /// </summary>
    /// <param name="event">The fact.</param>
    /// <param name="context">The <see cref="EventContext"/>.</param>
    /// <param name="delivery">The <see cref="ReactorDelivery"/>.</param>
    /// <returns>An awaitable task.</returns>
    public Task Unassigned(SeatUnassigned @event, EventContext context, ReactorDelivery delivery) => Report(@event, context, delivery);

    async Task Report(object fact, EventContext context, ReactorDelivery delivery)
    {
        var eventStore = await client.GetEventStore(context.EventStore, context.Namespace);

        // The organization is answered from the event log, not from a read model: a read model can be behind the facts.
        var identified = await eventStore.EventLog.GetForEventSourceIdAndEventTypes(
            TenantStream.Id,
            [eventStore.EventTypes.GetEventTypeFor(typeof(Receiving.TenantOrganizationIdentified))]);
        if (identified.Count == 0 || identified[^1].Content is not Receiving.TenantOrganizationIdentified { Organization: var organization })
        {
            throw new OrganizationNotKnown(context.Namespace);
        }

        var publicEvent = PublicEventFor(fact, new TenantId(context.Namespace.Value));
        var result = await eventStore.GetEventSequence(EventSequenceId.Outbox).AppendOnce(
            organization,
            publicEvent,
            delivery.IdempotencyKeyFor(nameof(Report)),
            correlationId: context.CorrelationId,
            occurred: context.Occurred);

        if (result.HasIdempotencyConflicts)
        {
            logger.AlreadyReportedDifferently(publicEvent.GetType().Name, organization, context.Namespace);
            return;
        }

        if (!result.IsSuccess)
        {
            throw new SeatNotReported(publicEvent.GetType(), organization, context.Namespace, result);
        }
    }
}
