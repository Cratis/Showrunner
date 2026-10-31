// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.Reactors;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Marquee.Authoring.Publishing;

/// <summary>
/// Publishes everything that happens to an announcement to the products, as the public contract events, in the order it
/// happened.
/// </summary>
/// <remarks>
/// <para>
/// It is the one reactor for all four facts, so a publication and a withdrawal of one announcement can not be handled in
/// the wrong order by two observers. It is not <c>[OnceOnly]</c>: every amendment is forwarded, and the publication of a
/// delivery carries an idempotency key, so a redelivery or a replay appends nothing a second time.
/// </para>
/// <para>
/// An announcement that was withdrawn is never left standing. When the reactor is handed a fact about an announcement
/// whose withdrawal is already recorded, for example when it catches up after being down, it publishes nothing for it,
/// and the withdrawal then retracts what the product never showed. A product treats a retraction of an announcement it
/// does not know as a retraction in advance.
/// </para>
/// <para>
/// All announcements of a product are appended under one event source, the product, so they share one product-wide
/// stream in the outbox and arrive in the order they were published. The announcement is a field of the event.
/// </para>
/// </remarks>
/// <param name="client">The Chronicle client, to reach the event log and the outbox of the namespace of the event.</param>
/// <param name="logger">The logger.</param>
[Reactor(Id)]
public class MarqueeOutbox(IChronicleClient client, ILogger<MarqueeOutbox> logger) : IReactor
{
    /// <summary>
    /// The stable identity of the reactor; its position in the event log and its idempotency keys hang on it.
    /// </summary>
    public const string Id = "Cratis.Showrunner.Marquee.Authoring.Publishing.MarqueeOutbox";

    /// <summary>
    /// Reacts to an announcement being authored.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="context">The context of the event.</param>
    /// <param name="delivery">The delivery being handled.</param>
    /// <returns>An awaitable task.</returns>
    public Task Authored(AnnouncementAuthored @event, EventContext context, ReactorDelivery delivery) =>
        Publish(context, delivery, _ => PublicEvents.For(AnnouncementIdOf(context), @event), _ => true);

    /// <summary>
    /// Reacts to an announcement being amended.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="context">The context of the event.</param>
    /// <param name="delivery">The delivery being handled.</param>
    /// <returns>An awaitable task.</returns>
    public Task Amended(AnnouncementAmended @event, EventContext context, ReactorDelivery delivery) =>
        Publish(context, delivery, authored => PublicEvents.For(AnnouncementIdOf(context), authored.Product, @event), _ => true);

    /// <summary>
    /// Reacts to an announcement being retargeted.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="context">The context of the event.</param>
    /// <param name="delivery">The delivery being handled.</param>
    /// <returns>An awaitable task.</returns>
    public Task Retargeted(AnnouncementRetargeted @event, EventContext context, ReactorDelivery delivery) =>
        Publish(context, delivery, authored => PublicEvents.For(AnnouncementIdOf(context), authored.Product, @event), _ => true);

    /// <summary>
    /// Reacts to an announcement being withdrawn.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="context">The context of the event.</param>
    /// <param name="delivery">The delivery being handled.</param>
    /// <returns>An awaitable task.</returns>
    public Task Withdrawn(AnnouncementWithdrawn @event, EventContext context, ReactorDelivery delivery) =>
        Publish(context, delivery, authored => PublicEvents.For(AnnouncementIdOf(context), authored.Product), _ => false);

    static AnnouncementId AnnouncementIdOf(EventContext context) => new(Guid.Parse(context.EventSourceId.Value));

    async Task Publish(EventContext context, ReactorDelivery delivery, Func<AnnouncementAuthored, object> publication, Func<AnnouncementWithdrawn?, bool> unlessWithdrawn)
    {
        var eventStore = await client.GetEventStore(context.EventStore, context.Namespace);
        var history = await eventStore.EventLog.GetForEventSourceIdAndEventTypes(
            context.EventSourceId,
            [eventStore.EventTypes.GetEventTypeFor(typeof(AnnouncementAuthored)), eventStore.EventTypes.GetEventTypeFor(typeof(AnnouncementWithdrawn))]);

        var authored = history.Select(appended => appended.Content).OfType<AnnouncementAuthored>().FirstOrDefault();
        var withdrawn = history.Select(appended => appended.Content).OfType<AnnouncementWithdrawn>().FirstOrDefault();
        if (authored is null)
        {
            logger.NeverAuthored(nameof(Publish), context.EventSourceId, context.Namespace);
            return;
        }

        // A withdrawn announcement is not shown again: only its withdrawal is published.
        if (withdrawn is not null && unlessWithdrawn(withdrawn))
        {
            return;
        }

        var publicEvent = publication(authored);
        var result = await eventStore.GetEventSequence(EventSequenceId.Outbox).AppendOnce(
            new EventSourceId(authored.Product.Value),
            publicEvent,
            delivery.IdempotencyKeyFor(nameof(Publish)),
            correlationId: context.CorrelationId,
            occurred: context.Occurred);

        if (result.HasIdempotencyConflicts)
        {
            logger.AlreadyPublishedDifferently(publicEvent.GetType().Name, context.EventSourceId, context.Namespace);
            return;
        }

        if (!result.IsSuccess)
        {
            throw new PublicationRefused(publicEvent.GetType(), context.EventSourceId, context.Namespace, result);
        }
    }
}
