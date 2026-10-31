// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Collections.Immutable;
using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.Reactors;
using Cratis.Chronicle.Testing.Events;
using Cratis.Showrunner.Marquee.Authoring.given;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Marquee.Authoring.Publishing.for_MarqueeOutbox.given;

/// <summary>
/// A <see cref="MarqueeOutbox"/> publishing the announcements recorded in an in-process event store, where each event is
/// appended to the event log and then delivered to the outbox the way Chronicle would deliver it.
/// </summary>
public class a_marquee_outbox : Specification
{
    protected EventStoreForTesting _eventStore;
    protected IChronicleClient _client;
    protected MarqueeOutbox _outbox;
    protected AnnouncementId _announcementId;
    protected IImmutableList<AppendedEvent> _published;

    void Establish()
    {
        _eventStore = new();
        _client = Substitute.For<IChronicleClient>();
        _client.GetEventStore(Arg.Any<EventStoreName>(), Arg.Any<EventStoreNamespaceName?>()).Returns(_eventStore);
        _outbox = new(_client, Substitute.For<ILogger<MarqueeOutbox>>());
        _announcementId = AnnouncementId.New();
    }

    void Destroy() => _eventStore.Dispose();

    /// <summary>
    /// Appends events to the announcement's stream in the event log without delivering them to the outbox.
    /// </summary>
    /// <param name="events">The events; arrays of events are flattened.</param>
    /// <returns>An awaitable task.</returns>
    protected async Task Recorded(params object[] events)
    {
        foreach (var @event in AnnouncementEvents.Then(events))
        {
            await _eventStore.GetEventSequence(EventSequenceId.Log).Append(_announcementId, @event);
        }
    }

    /// <summary>
    /// Appends an event to the announcement's stream in the event log and delivers it to the outbox.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <returns>An awaitable task.</returns>
    protected async Task Happened(object @event)
    {
        await Recorded(@event);
        var appended = (await _eventStore.GetEventSequence(EventSequenceId.Log).GetFromSequenceNumber(EventSequenceNumber.First))
            .Last(candidate => candidate.Context.EventSourceId == _announcementId);
        await Deliver(appended);
    }

    /// <summary>
    /// Delivers every event of the announcement that is already in the event log to the outbox, in order.
    /// </summary>
    /// <returns>An awaitable task.</returns>
    protected async Task DeliverEverything()
    {
        var log = await _eventStore.GetEventSequence(EventSequenceId.Log).GetFromSequenceNumber(EventSequenceNumber.First);
        foreach (var appended in log.Where(candidate => candidate.Context.EventSourceId == _announcementId))
        {
            await Deliver(appended);
        }
    }

    /// <summary>
    /// Delivers an event that is already in the event log to the outbox.
    /// </summary>
    /// <param name="appended">The event.</param>
    /// <returns>An awaitable task.</returns>
    protected async Task Deliver(AppendedEvent appended)
    {
        var delivery = ReactorDelivery.For(MarqueeOutbox.Id, EventSequenceId.Log, appended.Context);
        var task = appended.Content switch
        {
            AnnouncementAuthored @event => _outbox.Authored(@event, appended.Context, delivery),
            AnnouncementAmended @event => _outbox.Amended(@event, appended.Context, delivery),
            AnnouncementRetargeted @event => _outbox.Retargeted(@event, appended.Context, delivery),
            AnnouncementWithdrawn @event => _outbox.Withdrawn(@event, appended.Context, delivery),
            _ => Task.CompletedTask
        };
        await task;
    }

    /// <summary>
    /// Reads everything that was published to the products.
    /// </summary>
    /// <returns>An awaitable task.</returns>
    protected async Task ReadPublished() =>
        _published = await _eventStore.GetEventSequence(EventSequenceId.Outbox).GetFromSequenceNumber(EventSequenceNumber.First);
}
#endif
