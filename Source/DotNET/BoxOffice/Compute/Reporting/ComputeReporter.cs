// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.Reactors;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.BoxOffice.Compute.Reporting;

/// <summary>
/// Tells the operator that a tenant's allowance ran out, once per meter and period.
/// </summary>
/// <param name="client">The <see cref="IChronicleClient"/>.</param>
/// <param name="logger">The logger.</param>
[Reactor(Id)]
[OnceOnly]
public class ComputeReporter(IChronicleClient client, ILogger<ComputeReporter> logger) : IReactor
{
    /// <summary>
    /// The identity of the reactor.
    /// </summary>
    public const string Id = "Cratis.Showrunner.BoxOffice.Compute.Reporting.ComputeReporter";

    /// <summary>
    /// Maps the fact to what the operator is told.
    /// </summary>
    /// <param name="fact">The private fact.</param>
    /// <param name="tenant">The tenant.</param>
    /// <returns>The public event.</returns>
    public static AllowanceExhausted PublicEventFor(ComputeAllowanceReached fact, TenantId tenant) =>
        new(tenant, fact.Meter, fact.Day, fact.Overage);

    /// <summary>
    /// Reports an allowance that ran out.
    /// </summary>
    /// <param name="event">The fact.</param>
    /// <param name="context">The <see cref="EventContext"/>.</param>
    /// <param name="delivery">The <see cref="ReactorDelivery"/>.</param>
    /// <returns>An awaitable task.</returns>
    /// <exception cref="ComputeOrganizationNotKnown">The tenant has not been told which organization it is.</exception>
    /// <exception cref="AllowanceNotReported">The outbox refused the event.</exception>
    public async Task Reached(ComputeAllowanceReached @event, EventContext context, ReactorDelivery delivery)
    {
        var eventStore = await client.GetEventStore(context.EventStore, context.Namespace);

        // The organization is answered from the event log, not from a read model: a read model can be behind the facts.
        var identified = await eventStore.EventLog.GetForEventSourceIdAndEventTypes(
            TenantStream.Id,
            [eventStore.EventTypes.GetEventTypeFor(typeof(Receiving.TenantOrganizationIdentified))]);
        if (identified.Count == 0 || identified[^1].Content is not Receiving.TenantOrganizationIdentified { Organization: var organization })
        {
            throw new ComputeOrganizationNotKnown(context.Namespace);
        }

        var publicEvent = PublicEventFor(@event, new TenantId(context.Namespace.Value));
        var result = await eventStore.GetEventSequence(EventSequenceId.Outbox).AppendOnce(
            organization,
            publicEvent,
            delivery.IdempotencyKeyFor(nameof(Reached)),
            correlationId: context.CorrelationId,
            occurred: context.Occurred);

        if (result.HasIdempotencyConflicts)
        {
            logger.AlreadyReportedDifferently(organization, context.Namespace);
            return;
        }

        if (!result.IsSuccess)
        {
            throw new AllowanceNotReported(organization, context.Namespace, result);
        }
    }
}
