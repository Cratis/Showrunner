// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Collections.Immutable;
using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.Reactors;
using Cratis.Chronicle.Testing.Events;
using Cratis.Showrunner.BoxOffice.Catalog.given;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.BoxOffice.Catalog.Publishing.for_OfferPublisher.given;

/// <summary>
/// An <see cref="OfferPublisher"/> publishing the offers recorded in an in-process event store, where each event is
/// appended to the event log and then delivered to the publisher the way Chronicle would deliver it.
/// </summary>
public class an_offer_publisher : Specification
{
    protected EventStoreForTesting _eventStore;
    protected IChronicleClient _client;
    protected OfferPublisher _publisher;
    protected OfferId _offerId;
    protected IImmutableList<AppendedEvent> _outbox;

    void Establish()
    {
        _eventStore = new();
        _client = Substitute.For<IChronicleClient>();
        _client.GetEventStore(Arg.Any<EventStoreName>(), Arg.Any<EventStoreNamespaceName?>()).Returns(_eventStore);
        _publisher = new(_client, Substitute.For<ILogger<OfferPublisher>>());
        _offerId = OfferId.New();
    }

    void Destroy() => _eventStore.Dispose();

    /// <summary>
    /// Appends events to the offer's stream in the event log without delivering them to the publisher.
    /// </summary>
    /// <param name="events">The events; arrays of events are flattened.</param>
    /// <returns>An awaitable task.</returns>
    protected async Task Recorded(params object[] events)
    {
        foreach (var @event in OfferEvents.Then(events))
        {
            await _eventStore.GetEventSequence(EventSequenceId.Log).Append(_offerId, @event);
        }
    }

    /// <summary>
    /// Appends an event to the offer's stream in the event log and delivers it to the publisher.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <returns>An awaitable task.</returns>
    protected async Task Happened(object @event)
    {
        await Recorded(@event);
        var appended = (await _eventStore.GetEventSequence(EventSequenceId.Log).GetFromSequenceNumber(EventSequenceNumber.First))
            .Last(candidate => candidate.Context.EventSourceId == _offerId);
        await Deliver(appended);
    }

    /// <summary>
    /// Delivers an event that is already in the event log to the publisher.
    /// </summary>
    /// <param name="appended">The event.</param>
    /// <returns>An awaitable task.</returns>
    protected async Task Deliver(AppendedEvent appended)
    {
        var delivery = ReactorDelivery.For(OfferPublisher.Id, EventSequenceId.Log, appended.Context);
        var task = appended.Content switch
        {
            OfferDefined @event => _publisher.DefinedOffer(@event, appended.Context, delivery),
            SeatPlanSpecified @event => _publisher.SpecifiedSeatPlan(@event, appended.Context, delivery),
            ModuleOfferSpecified @event => _publisher.SpecifiedModuleOffer(@event, appended.Context, delivery),
            OfferPriced @event => _publisher.PricedOffer(@event, appended.Context, delivery),
            OfferIncludedModule @event => _publisher.IncludedModuleInOffer(@event, appended.Context, delivery),
            OfferRetired @event => _publisher.RetiredOffer(@event, appended.Context, delivery),
            _ => Task.CompletedTask
        };
        await task;
    }

    /// <summary>
    /// Reads everything in the outbox.
    /// </summary>
    /// <returns>An awaitable task.</returns>
    protected async Task ReadOutbox() =>
        _outbox = await _eventStore.GetEventSequence(EventSequenceId.Outbox).GetFromSequenceNumber(EventSequenceNumber.First);
}
#endif
