// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Collections.Immutable;
using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.Reactors;
using Cratis.Chronicle.Testing.Events;
using Cratis.Execution;
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Seats.Reporting.for_SeatReporter.given;

/// <summary>
/// A <see cref="SeatReporter"/> reporting the seats of a tenant to the outbox of the tenant's namespace.
/// </summary>
public class a_seat_reporter : Specification
{
    /// <summary>
    /// The namespace the facts are recorded in, which is the tenant.
    /// </summary>
    protected static readonly EventStoreNamespaceName Tenant = new(Tenants.Tenant.Value);

    protected EventStoreForTesting _eventStore;
    protected IChronicleClient _client;
    protected RecordingLogger<SeatReporter> _logger;
    protected SeatReporter _reporter;
    protected CorrelationId _correlationId;
    protected IImmutableList<AppendedEvent> _outbox;

    void Establish()
    {
        _eventStore = new();
        _client = Substitute.For<IChronicleClient>();
        _client.GetEventStore(Arg.Any<EventStoreName>(), Arg.Any<EventStoreNamespaceName?>()).Returns(_eventStore);
        _logger = new();
        _reporter = new(_client, _logger);
        _correlationId = CorrelationId.New();
    }

    void Destroy() => _eventStore.Dispose();

    /// <summary>
    /// Records that the operator's organization behind the tenant is known.
    /// </summary>
    /// <returns>An awaitable task.</returns>
    protected Task OrganizationIsKnown() => _eventStore.EventLog.Append(TenantStream.Id, new TenantOrganizationIdentified(Tenants.Organization));

    /// <summary>
    /// Creates the context of a seat fact recorded in the tenant's stream.
    /// </summary>
    /// <param name="sequenceNumber">The sequence number of the fact in the event log.</param>
    /// <returns>The context.</returns>
    protected EventContext ContextFor(ulong sequenceNumber = 0) => EventContext.From(
        "Ledger",
        Tenant,
        EventType.Unknown,
        EventSourceType.Default,
        TenantStream.Id,
        EventStreamType.All,
        EventStreamId.Default,
        sequenceNumber,
        _correlationId,
        Tenants.Start);

    /// <summary>
    /// Creates the delivery of a fact to the reporter.
    /// </summary>
    /// <param name="context">The context of the fact.</param>
    /// <returns>The delivery.</returns>
    protected static ReactorDelivery DeliveryOf(EventContext context) => ReactorDelivery.For(SeatReporter.Id, EventSequenceId.Log, context);

    /// <summary>
    /// Reads everything in the outbox.
    /// </summary>
    /// <returns>An awaitable task.</returns>
    protected async Task ReadOutbox() =>
        _outbox = await _eventStore.GetEventSequence(EventSequenceId.Outbox).GetFromSequenceNumber(EventSequenceNumber.First);
}
#endif
