// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Collections.Immutable;
using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.Testing.Events;
using Cratis.Execution;
using Cratis.Showrunner.GuestList.Contracts.Availability;
using Cratis.Showrunner.GuestList.given;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.GuestList.Receiving.for_AudienceReceipts.given;

/// <summary>
/// The receipts of the product <c>ledger</c>, which reports from the environment <c>Staging</c>, and the outbox they publish to.
/// </summary>
public class receipts_for_a_product : Specification
{
    protected static readonly EventStoreName ProductStore = new("Ledger");

    protected EventStoreForTesting _eventStore;
    protected AudienceReceipts _receipts;
    protected IImmutableList<AppendedEvent> _published;
    ulong _sequenceNumber;

    void Establish()
    {
        _eventStore = new();
        var client = Substitute.For<IChronicleClient>();
        client.GetEventStore(ProductStore, Arg.Is<EventStoreNamespaceName?>(_ => _ == EventStoreNamespaceName.Default)).Returns(_eventStore);
        _receipts = new(
            client,
            Options.Create(new GuestListOptions { Product = Guests.Product, Environment = "Staging" }),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<AudienceReceipts>.Instance);
    }

    void Destroy() => _eventStore.Dispose();

    /// <summary>
    /// Gets the context the product's own event has when it records a decision.
    /// </summary>
    /// <param name="namespace">The namespace it is recorded in.</param>
    /// <param name="stream">The stream it is recorded in.</param>
    /// <param name="occurred">When the operator decided.</param>
    /// <returns>The context.</returns>
    protected EventContext Recorded(EventStoreNamespaceName @namespace, EventSourceId stream, DateTimeOffset occurred) =>
        EventContext.From(
            ProductStore,
            @namespace,
            EventType.Unknown,
            EventSourceType.Default,
            stream,
            EventStreamType.All,
            EventStreamId.Default,
            _sequenceNumber++,
            CorrelationId.New(),
            occurred);

    /// <summary>
    /// Reads everything the product published to the operator.
    /// </summary>
    /// <returns>An awaitable task.</returns>
    protected async Task ReadPublished() =>
        _published = await _eventStore.GetEventSequence(EventSequenceId.Outbox).GetFromSequenceNumber(EventSequenceNumber.First);

    /// <summary>
    /// Gets a published receipt.
    /// </summary>
    /// <param name="index">Which one, in the order they were published.</param>
    /// <returns>The receipt.</returns>
    protected AudienceDecisionReceived Receipt(int index = 0) => (AudienceDecisionReceived)_published[index].Content;
}
#endif
