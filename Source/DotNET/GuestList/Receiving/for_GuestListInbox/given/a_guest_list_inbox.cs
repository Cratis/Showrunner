// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Collections.Immutable;
using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.Testing.Events;
using Cratis.Execution;
using Cratis.Showrunner.GuestList.given;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.GuestList.Receiving.for_GuestListInbox.given;

/// <summary>
/// A <see cref="GuestListInbox"/> for the product <c>ledger</c>, recording what the <c>Direct</c> operator store publishes.
/// The product has the default namespace, where the system's audiences are recorded, and the tenant namespace
/// <c>acme</c>; each has an in-process event store.
/// </summary>
public class a_guest_list_inbox : Specification
{
    protected static readonly EventStoreName OperatorStore = new("Direct");
    protected static readonly EventStoreName ProductStore = new("Ledger");

    protected EventStoreForTesting _productStore;
    protected EventStoreForTesting _acme;
    protected IEventStore _inbox;
    protected IChronicleClient _client;
    protected GuestListInbox _guestList;
    protected IImmutableList<AppendedEvent> _recordedForTheProduct;
    protected IImmutableList<AppendedEvent> _recordedForAcme;
    DateTimeOffset _occurred = Guests.Start;
    ulong _sequenceNumber;

    /// <summary>
    /// Gets the namespaces the product has.
    /// </summary>
    protected virtual IEnumerable<EventStoreNamespaceName> Namespaces => [EventStoreNamespaceName.Default, new(Guests.Acme.Value)];

    void Establish()
    {
        _productStore = new();
        _acme = new();
        _inbox = Substitute.For<IEventStore>();
        _inbox.GetNamespaces(Arg.Any<CancellationToken>()).Returns(_ => Namespaces);
        _inbox.EventLog.Returns(_productStore.EventLog);
        _client = Substitute.For<IChronicleClient>();
        _client.GetEventStore(ProductStore, Arg.Is<EventStoreNamespaceName?>(_ => _ == EventStoreNamespaceName.Default)).Returns(_inbox);
        _client.GetEventStore(ProductStore, Arg.Is<EventStoreNamespaceName?>(_ => _ == new EventStoreNamespaceName(Guests.Acme.Value))).Returns(_acme);
        _guestList = new(_client, Options.Create(new GuestListOptions { Product = Guests.Product }), Substitute.For<ILogger<GuestListInbox>>());
    }

    void Destroy()
    {
        _productStore.Dispose();
        _acme.Dispose();
    }

    /// <summary>
    /// Gets the context an event has in the product's inbox: the operator store published it in its default namespace under
    /// the product.
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
    /// <param name="occurred">When the operator decided.</param>
    /// <param name="correlationId">What it is correlated with.</param>
    /// <returns>The context.</returns>
    protected static EventContext Context(ulong sequenceNumber, DateTimeOffset occurred, CorrelationId correlationId) =>
        EventContext.From(
            ProductStore,
            EventStoreNamespaceName.Default,
            EventType.Unknown,
            EventSourceType.Default,
            new EventSourceId(Guests.Product.Value),
            EventStreamType.All,
            EventStreamId.Default,
            sequenceNumber,
            correlationId,
            occurred);

    /// <summary>
    /// Delivers an event the operator store published to the inbox, as Chronicle does.
    /// </summary>
    /// <param name="event">The public event.</param>
    /// <returns>An awaitable task.</returns>
    protected Task Delivered(object @event) => Delivered(@event, NextContext());

    /// <summary>
    /// Delivers an event the operator store published to the inbox with a context it already had.
    /// </summary>
    /// <param name="event">The public event.</param>
    /// <param name="context">The context of the event.</param>
    /// <returns>An awaitable task.</returns>
    protected Task Delivered(object @event, EventContext context) => @event switch
    {
        AudienceGranted granted => _guestList.Receive(granted, context, OperatorStore),
        AudienceWithdrawn withdrawn => _guestList.Receive(withdrawn, context, OperatorStore),
        _ => Task.CompletedTask
    };

    /// <summary>
    /// Reads everything the product recorded, in its own namespace and in the tenant's.
    /// </summary>
    /// <returns>An awaitable task.</returns>
    protected async Task ReadRecorded()
    {
        _recordedForTheProduct = await _productStore.GetEventSequence(EventSequenceId.Log).GetFromSequenceNumber(EventSequenceNumber.First);
        _recordedForAcme = await _acme.GetEventSequence(EventSequenceId.Log).GetFromSequenceNumber(EventSequenceNumber.First);
    }
}
#endif
