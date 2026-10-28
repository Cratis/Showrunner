// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Collections.Immutable;
using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.Reactors;
using Cratis.Chronicle.Testing.Events;
using Cratis.Execution;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Publishing.for_OutboxPublisher.given;

/// <summary>
/// An <see cref="OutboxPublisher"/> publishing the facts recorded in a tenant's namespace to an in-process event store.
/// </summary>
public class an_outbox_publisher : Specification
{
    /// <summary>
    /// The namespace the facts are recorded in, which is the tenant.
    /// </summary>
    protected static readonly EventStoreNamespaceName Tenant = "fjord-logistics";

    /// <summary>
    /// When the private facts happened.
    /// </summary>
    protected static readonly DateTimeOffset Occurred = new(2026, 10, 26, 9, 30, 0, TimeSpan.Zero);

    protected EventStoreForTesting _eventStore;
    protected IChronicleClient _client;
    protected RecordingLogger<OutboxPublisher> _logger;
    protected OutboxPublisher _publisher;
    protected IssueId _issueId;
    protected CorrelationId _correlationId;
    protected IImmutableList<AppendedEvent> _outbox;

    void Establish()
    {
        _eventStore = new();
        _client = Substitute.For<IChronicleClient>();
        _client.GetEventStore(Arg.Any<EventStoreName>(), Arg.Any<EventStoreNamespaceName?>()).Returns(_eventStore);
        _logger = new();
        _publisher = new(_client, _logger);
        _issueId = IssueId.New();
        _correlationId = CorrelationId.New();
    }

    void Destroy() => _eventStore.Dispose();

    /// <summary>
    /// Creates the context of a private fact recorded on the issue in the tenant's namespace.
    /// </summary>
    /// <param name="sequenceNumber">The sequence number of the fact in the event log.</param>
    /// <returns>The context.</returns>
    protected EventContext ContextFor(ulong sequenceNumber = 0) => EventContext.From(
        "Ledger",
        Tenant,
        EventType.Unknown,
        EventSourceType.Default,
        _issueId,
        EventStreamType.All,
        EventStreamId.Default,
        sequenceNumber,
        _correlationId,
        Occurred);

    /// <summary>
    /// Creates the delivery of a private fact to the publisher.
    /// </summary>
    /// <param name="context">The context of the fact.</param>
    /// <returns>The delivery.</returns>
    protected static ReactorDelivery DeliveryOf(EventContext context) => ReactorDelivery.For(OutboxPublisher.Id, EventSequenceId.Log, context);

    /// <summary>
    /// Reads everything in the outbox.
    /// </summary>
    /// <returns>An awaitable task.</returns>
    protected async Task ReadOutbox() =>
        _outbox = await _eventStore.GetEventSequence(EventSequenceId.Outbox).GetFromSequenceNumber(EventSequenceNumber.First);
}
#endif
