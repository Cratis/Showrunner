// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Marquee.Receiving;

/// <summary>
/// Records what the authoring store published about an announcement: turns a public event that arrived in the inbox into
/// the matching private fact and appends it to the product-wide <c>marquee</c> stream, in the product's own namespace,
/// under the announcement's id.
/// </summary>
/// <remarks>
/// <para>
/// Chronicle's forwarder delivers an event at least once, so the same event can arrive twice, and the inbox can be handed
/// the same event again after a failure. The private fact is appended with an idempotency key derived from the event
/// itself - the authoring store, the announcement, the event type, when it occurred, its correlation id and its content -
/// so every copy of the same event yields one fact.
/// </para>
/// <para>
/// A retracted announcement is never brought back. A retraction is recorded also for an announcement the product never
/// received, so when the retraction arrives first, what arrives about the announcement afterwards is dropped. A revision
/// or a new audience for an announcement that was never received is dropped, and so is a second publication of one that
/// was.
/// </para>
/// <para>
/// All announcements of a product arrive under one event source, the product, so the inbox handles them one at a time
/// in the order they were published, and the check of what is recorded before an append can not race another append of
/// the same announcement.
/// </para>
/// </remarks>
/// <param name="client">The Chronicle client, to reach the event log of the product's own namespace.</param>
/// <param name="options">Marquee's options, which name the product.</param>
/// <param name="logger">The logger.</param>
public class MarqueeInbox(IChronicleClient client, IOptions<MarqueeOptions> options, ILogger<MarqueeInbox> logger)
{
    /// <summary>
    /// Derives the idempotency key of an event from the authoring store: the same for every copy of the event the
    /// forwarder delivers.
    /// </summary>
    /// <param name="authoringEvent">The public event the authoring store sent.</param>
    /// <param name="context">The context of the event in the inbox.</param>
    /// <param name="authoringStore">The authoring store the event came from.</param>
    /// <returns>The key.</returns>
    public static IdempotencyKey KeyFor(object authoringEvent, EventContext context, EventStoreName authoringStore)
    {
        var identity = string.Join(
            '\n',
            authoringStore.Value,
            context.EventSourceId.Value,
            authoringEvent.GetType().FullName,
            context.Occurred.UtcTicks.ToString(CultureInfo.InvariantCulture),
            context.CorrelationId.Value.ToString("D"),
            JsonSerializer.Serialize(authoringEvent, authoringEvent.GetType()));
        return new($"marquee-{Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(identity)))}");
    }

    /// <summary>
    /// Receives an announcement being published.
    /// </summary>
    /// <param name="event">The public event.</param>
    /// <param name="context">The context of the event in the inbox.</param>
    /// <param name="authoringStore">The authoring store the event came from.</param>
    /// <returns>An awaitable task.</returns>
    public Task Receive(AnnouncementPublished @event, EventContext context, EventStoreName authoringStore) =>
        Record(
            @event,
            @event.AnnouncementId,
            @event.Product,
            new AnnouncementReceived(@event.Audience, @event.Title, @event.Body, @event.Display, @event.Importance, @event.ActiveFrom, @event.ActiveUntil),
            context,
            authoringStore,
            known => !known.Retracted && !known.Received);

    /// <summary>
    /// Receives an announcement being revised.
    /// </summary>
    /// <param name="event">The public event.</param>
    /// <param name="context">The context of the event in the inbox.</param>
    /// <param name="authoringStore">The authoring store the event came from.</param>
    /// <returns>An awaitable task.</returns>
    public Task Receive(AnnouncementRevised @event, EventContext context, EventStoreName authoringStore) =>
        Record(
            @event,
            @event.AnnouncementId,
            @event.Product,
            new AnnouncementRevisionReceived(@event.Title, @event.Body, @event.Importance, @event.ActiveFrom, @event.ActiveUntil),
            context,
            authoringStore,
            known => !known.Retracted && known.Received);

    /// <summary>
    /// Receives an announcement being given another audience.
    /// </summary>
    /// <param name="event">The public event.</param>
    /// <param name="context">The context of the event in the inbox.</param>
    /// <param name="authoringStore">The authoring store the event came from.</param>
    /// <returns>An awaitable task.</returns>
    public Task Receive(AnnouncementAudienceChanged @event, EventContext context, EventStoreName authoringStore) =>
        Record(
            @event,
            @event.AnnouncementId,
            @event.Product,
            new AnnouncementAudienceChangeReceived(@event.Audience),
            context,
            authoringStore,
            known => !known.Retracted && known.Received);

    /// <summary>
    /// Receives an announcement being retracted.
    /// </summary>
    /// <param name="event">The public event.</param>
    /// <param name="context">The context of the event in the inbox.</param>
    /// <param name="authoringStore">The authoring store the event came from.</param>
    /// <returns>An awaitable task.</returns>
    public Task Receive(AnnouncementRetracted @event, EventContext context, EventStoreName authoringStore) =>
        Record(
            @event,
            @event.AnnouncementId,
            @event.Product,
            new AnnouncementRetractionReceived(),
            context,
            authoringStore,
            known => !known.Retracted);

    async Task Record(
        object authoringEvent,
        AnnouncementId announcementId,
        Product product,
        object received,
        EventContext context,
        EventStoreName authoringStore,
        Func<Known, bool> shouldRecord)
    {
        var eventType = authoringEvent.GetType().Name;
        if (product != options.Value.Product)
        {
            logger.ForAnotherProduct(eventType, announcementId, authoringStore, product.Value);
            return;
        }

        var eventStore = await client.GetEventStore(context.EventStore, MarqueeStream.Namespace);
        var history = await eventStore.EventLog.GetForEventSourceIdAndEventTypes(
            announcementId,
            [
                eventStore.EventTypes.GetEventTypeFor(typeof(AnnouncementReceived)),
                eventStore.EventTypes.GetEventTypeFor(typeof(AnnouncementRetractionReceived))
            ]);
        var known = new Known(
            history.Any(appended => appended.Content is AnnouncementReceived),
            history.Any(appended => appended.Content is AnnouncementRetractionReceived));

        if (!shouldRecord(known))
        {
            Explain(eventType, announcementId, authoringStore, known, received);
            return;
        }

        logger.Recording(eventType, announcementId, authoringStore);
        var result = await eventStore.EventLog.AppendOnce(
            announcementId,
            received,
            KeyFor(authoringEvent, context, authoringStore),
            eventSourceType: MarqueeStream.SourceType,
            correlationId: context.CorrelationId,
            occurred: context.Occurred);

        if (result.WasAlreadyAppended)
        {
            logger.AlreadyRecorded(eventType, announcementId, authoringStore);
            return;
        }

        if (!result.IsSuccess)
        {
            throw new AnnouncementNotRecorded(authoringEvent.GetType(), announcementId, authoringStore, result);
        }
    }

    void Explain(string eventType, EventSourceId announcementId, EventStoreName authoringStore, Known known, object received)
    {
        if (known.Retracted)
        {
            logger.AlreadyRetracted(eventType, announcementId, authoringStore);
        }
        else if (known.Received && received is AnnouncementReceived)
        {
            logger.AlreadyReceived(eventType, announcementId, authoringStore);
        }
        else
        {
            logger.NeverReceived(eventType, announcementId, authoringStore);
        }
    }

    readonly record struct Known(bool Received, bool Retracted);
}
