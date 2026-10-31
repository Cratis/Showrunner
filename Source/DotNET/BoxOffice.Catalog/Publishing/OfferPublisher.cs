// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.Reactors;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.BoxOffice.Catalog.Publishing;

/// <summary>
/// Publishes the catalog to the products that sell it: when what an offer says to a product changes, the
/// <see cref="OfferPublished"/> for it goes to the outbox of the namespace it was recorded in, under the same offer
/// id; when a published offer is retired, <see cref="OfferWithdrawn"/> does.
/// </summary>
/// <remarks>
/// <para>
/// It publishes only a change. For each event it folds the offer's own events up to and including the event and up to
/// but excluding it, and publishes only when the two public faces differ, so an event that does not change what a
/// product sees - an allowance, a price set to what it already was - publishes nothing. An offer that is not priced, or
/// is retired, has no public face; retiring one that had one withdraws it.
/// </para>
/// <para>
/// Chronicle delivers an event at least once. Each publication is appended with an idempotency key derived from the
/// delivery, so a repeated delivery finds its publication in the outbox and appends nothing, and the reactor is
/// <see cref="OnceOnlyAttribute">not replayable</see>. A refused append throws, so the partition fails and the
/// publication is retried rather than lost.
/// </para>
/// </remarks>
/// <param name="client">The Chronicle client, to reach the event log and outbox of the namespace the event was recorded in.</param>
/// <param name="logger">The logger.</param>
[Reactor(Id)]
[OnceOnly]
public class OfferPublisher(IChronicleClient client, ILogger<OfferPublisher> logger) : IReactor
{
    /// <summary>
    /// The stable identity of the reactor; its position in the event log and its idempotency keys hang on it.
    /// </summary>
    public const string Id = "Cratis.Showrunner.BoxOffice.Catalog.Publishing.OfferPublisher";

    /// <summary>
    /// Reacts to an offer being defined.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="context">The context of the event.</param>
    /// <param name="delivery">The delivery being handled.</param>
    /// <returns>An awaitable task.</returns>
    public Task DefinedOffer(OfferDefined @event, EventContext context, ReactorDelivery delivery) => Publish(context, delivery);

    /// <summary>
    /// Reacts to a seat plan being given its seats.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="context">The context of the event.</param>
    /// <param name="delivery">The delivery being handled.</param>
    /// <returns>An awaitable task.</returns>
    public Task SpecifiedSeatPlan(SeatPlanSpecified @event, EventContext context, ReactorDelivery delivery) => Publish(context, delivery);

    /// <summary>
    /// Reacts to a module offer being given its module.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="context">The context of the event.</param>
    /// <param name="delivery">The delivery being handled.</param>
    /// <returns>An awaitable task.</returns>
    public Task SpecifiedModuleOffer(ModuleOfferSpecified @event, EventContext context, ReactorDelivery delivery) => Publish(context, delivery);

    /// <summary>
    /// Reacts to an offer being priced.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="context">The context of the event.</param>
    /// <param name="delivery">The delivery being handled.</param>
    /// <returns>An awaitable task.</returns>
    public Task PricedOffer(OfferPriced @event, EventContext context, ReactorDelivery delivery) => Publish(context, delivery);

    /// <summary>
    /// Reacts to an offer including a module.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="context">The context of the event.</param>
    /// <param name="delivery">The delivery being handled.</param>
    /// <returns>An awaitable task.</returns>
    public Task IncludedModuleInOffer(OfferIncludedModule @event, EventContext context, ReactorDelivery delivery) => Publish(context, delivery);

    /// <summary>
    /// Reacts to an offer being retired.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="context">The context of the event.</param>
    /// <param name="delivery">The delivery being handled.</param>
    /// <returns>An awaitable task.</returns>
    public Task RetiredOffer(OfferRetired @event, EventContext context, ReactorDelivery delivery) => Publish(context, delivery);

    async Task Publish(EventContext context, ReactorDelivery delivery)
    {
        var eventStore = await client.GetEventStore(context.EventStore, context.Namespace);
        var eventTypes = OfferHistory.PublicationEvents.Select(type => type.GetEventType()).ToArray();
        var history = await eventStore.GetEventSequence(EventSequenceId.Log).GetForEventSourceIdAndEventTypes(context.EventSourceId, eventTypes);
        var upToAndIncluding = history.Where(appended => appended.Context.SequenceNumber <= context.SequenceNumber).ToArray();
        var before = OfferPublications.For(OfferHistory.Fold(upToAndIncluding.Where(appended => appended.Context.SequenceNumber < context.SequenceNumber)));
        var after = OfferPublications.For(OfferHistory.Fold(upToAndIncluding));

        object publication;
        if (after is not null)
        {
            if (before is not null && OfferPublications.Same(before, after))
            {
                return;
            }

            publication = after;
        }
        else if (before is not null)
        {
            publication = new OfferWithdrawn();
        }
        else
        {
            return;
        }

        var result = await eventStore.GetEventSequence(EventSequenceId.Outbox).AppendOnce(
            context.EventSourceId,
            publication,
            delivery.IdempotencyKeyFor(nameof(Publish)),
            correlationId: context.CorrelationId,
            occurred: context.Occurred,
            subject: context.Subject);

        if (result.HasIdempotencyConflicts)
        {
            logger.AlreadyPublishedDifferently(publication.GetType().Name, context.EventSourceId, context.Namespace);
            return;
        }

        if (!result.IsSuccess)
        {
            throw new PublicationRefused(publication.GetType(), context.EventSourceId, context.Namespace, result);
        }
    }
}
