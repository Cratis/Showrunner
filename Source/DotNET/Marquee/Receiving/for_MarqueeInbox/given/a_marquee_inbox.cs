// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Collections.Immutable;
using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.Testing.Events;
using Cratis.Execution;
using Cratis.Showrunner.Marquee.given;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Marquee.Receiving.for_MarqueeInbox.given;

/// <summary>
/// A <see cref="MarqueeInbox"/> for the product <c>ledger</c> recording what the authoring store <c>Direct</c> publishes
/// in an in-process event store, which stands for the product's own namespace.
/// </summary>
public class a_marquee_inbox : Specification
{
    protected static readonly EventStoreName AuthoringStore = new("Direct");
    protected static readonly EventStoreName ProductStore = new("Ledger");

    protected EventStoreForTesting _eventStore;
    protected IChronicleClient _client;
    protected MarqueeInbox _inbox;
    protected AnnouncementId _announcementId;
    protected IImmutableList<AppendedEvent> _recorded;
    DateTimeOffset _occurred = new(2026, 11, 2, 9, 0, 0, TimeSpan.Zero);
    ulong _sequenceNumber;

    void Establish()
    {
        _eventStore = new();
        _client = Substitute.For<IChronicleClient>();
        _client.GetEventStore(Arg.Any<EventStoreName>(), Arg.Any<EventStoreNamespaceName?>()).Returns(_eventStore);
        _inbox = new(_client, Options.Create(new MarqueeOptions { Product = ProductAnnouncements.Product }), Substitute.For<ILogger<MarqueeInbox>>());
        _announcementId = AnnouncementId.New();
    }

    void Destroy() => _eventStore.Dispose();

    /// <summary>
    /// Gets the context an event has in the product's inbox: the authoring store published it under the product.
    /// </summary>
    /// <returns>The context, for the next event; the same context is used when an event is delivered again.</returns>
    protected EventContext NextContext()
    {
        var sequenceNumber = _sequenceNumber++;
        _occurred = _occurred.AddMinutes(1);
        return Context(sequenceNumber, _occurred, CorrelationId.New());
    }

    /// <summary>
    /// Gets the context an event has in the inbox.
    /// </summary>
    /// <param name="sequenceNumber">Where the event is in the inbox.</param>
    /// <param name="occurred">When it happened.</param>
    /// <param name="correlationId">What it is correlated with.</param>
    /// <returns>The context.</returns>
    protected static EventContext Context(ulong sequenceNumber, DateTimeOffset occurred, CorrelationId correlationId) =>
        EventContext.From(
            ProductStore,
            EventStoreNamespaceName.Default,
            EventType.Unknown,
            EventSourceType.Default,
            new EventSourceId(ProductAnnouncements.Product.Value),
            EventStreamType.All,
            EventStreamId.Default,
            sequenceNumber,
            correlationId,
            occurred);

    /// <summary>
    /// Delivers an event the authoring store published to the inbox, as Chronicle does.
    /// </summary>
    /// <param name="event">The public event.</param>
    /// <returns>An awaitable task.</returns>
    protected Task Delivered(object @event) => Delivered(@event, NextContext());

    /// <summary>
    /// Delivers an event the authoring store published to the inbox with a context it already had.
    /// </summary>
    /// <param name="event">The public event.</param>
    /// <param name="context">The context of the event.</param>
    /// <returns>An awaitable task.</returns>
    protected Task Delivered(object @event, EventContext context) => @event switch
    {
        AnnouncementPublished published => _inbox.Receive(published, context, AuthoringStore),
        AnnouncementRevised revised => _inbox.Receive(revised, context, AuthoringStore),
        AnnouncementAudienceChanged changed => _inbox.Receive(changed, context, AuthoringStore),
        AnnouncementRetracted retracted => _inbox.Receive(retracted, context, AuthoringStore),
        _ => Task.CompletedTask
    };

    /// <summary>
    /// Reads everything the product recorded.
    /// </summary>
    /// <returns>An awaitable task.</returns>
    protected async Task ReadRecorded() =>
        _recorded = await _eventStore.GetEventSequence(EventSequenceId.Log).GetFromSequenceNumber(EventSequenceNumber.First);

    /// <summary>
    /// Gets the public event for an announcement being published.
    /// </summary>
    /// <returns>The event.</returns>
    protected AnnouncementPublished Published() => ProductAnnouncements.Published(ProductAnnouncements.Banner(_announcementId));

    /// <summary>
    /// Gets the public event for an announcement being revised.
    /// </summary>
    /// <returns>The event.</returns>
    protected AnnouncementRevised Revised() =>
        new(_announcementId, ProductAnnouncements.Product, "Moved", "Later.", Importance.Critical, ProductAnnouncements.MaintenanceEnds, ActiveUntil.Indefinitely);

    /// <summary>
    /// Gets the public event for an announcement being given another audience.
    /// </summary>
    /// <returns>The event.</returns>
    protected AnnouncementAudienceChanged AudienceChanged() => new(_announcementId, ProductAnnouncements.Product, Audience.OnPlan("team"));

    /// <summary>
    /// Gets the public event for an announcement being retracted.
    /// </summary>
    /// <returns>The event.</returns>
    protected AnnouncementRetracted Retracted() => new(_announcementId, ProductAnnouncements.Product);
}
#endif
