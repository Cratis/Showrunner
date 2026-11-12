// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Collections.Immutable;
using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.Reactors;
using Cratis.Chronicle.Testing.Events;
using Cratis.Showrunner.GuestList.Authoring.given;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.GuestList.Authoring.Publishing.for_GuestListOutbox.given;

/// <summary>
/// A <see cref="GuestListOutbox"/> publishing the decisions recorded in an in-process event store, where each event is
/// appended to the event log and then delivered to the outbox the way Chronicle would deliver it.
/// </summary>
public class a_guest_list_outbox : Specification
{
    protected EventStoreForTesting _eventStore;
    protected IChronicleClient _client;
    protected GuestListOutbox _outbox;
    protected IImmutableList<AppendedEvent> _published;

    void Establish()
    {
        _eventStore = new();
        _client = Substitute.For<IChronicleClient>();
        _client.GetEventStore(Arg.Any<EventStoreName>(), Arg.Any<EventStoreNamespaceName?>()).Returns(_eventStore);
        _outbox = new(_client, Substitute.For<ILogger<GuestListOutbox>>());
    }

    void Destroy() => _eventStore.Dispose();

    /// <summary>
    /// Appends events to the product's stream in the event log without delivering them to the outbox.
    /// </summary>
    /// <param name="events">The events.</param>
    /// <returns>An awaitable task.</returns>
    protected async Task Recorded(params object[] events)
    {
        foreach (var @event in events)
        {
            await _eventStore.GetEventSequence(EventSequenceId.Log).Append(new EventSourceId(Guests.Product.Value), @event);
        }
    }

    /// <summary>
    /// Appends an event to the product's stream in the event log and delivers it to the outbox.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <returns>An awaitable task.</returns>
    protected async Task Happened(object @event)
    {
        await Recorded(@event);
        var log = await _eventStore.GetEventSequence(EventSequenceId.Log).GetFromSequenceNumber(EventSequenceNumber.First);
        var appended = log[log.Count - 1];
        await Deliver(appended);
    }

    /// <summary>
    /// Delivers every event that is already in the event log to the outbox, in order.
    /// </summary>
    /// <returns>An awaitable task.</returns>
    protected async Task DeliverEverything()
    {
        foreach (var appended in await _eventStore.GetEventSequence(EventSequenceId.Log).GetFromSequenceNumber(EventSequenceNumber.First))
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
        var delivery = ReactorDelivery.For(GuestListOutbox.Id, EventSequenceId.Log, appended.Context);
        var task = appended.Content switch
        {
            AudienceGrantDecided @event => _outbox.Granted(@event, appended.Context, delivery),
            AudienceWithdrawalDecided @event => _outbox.Withdrawn(@event, appended.Context, delivery),
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
