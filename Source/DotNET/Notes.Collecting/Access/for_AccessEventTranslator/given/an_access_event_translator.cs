// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Collections.Immutable;
using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.Testing.Events;
using Cratis.Showrunner.Notes.Collecting.Access.given;
using Cratis.Showrunner.Notes.Collecting.given;

namespace Cratis.Showrunner.Notes.Collecting.Access.for_AccessEventTranslator.given;

/// <summary>
/// An <see cref="AccessEventTranslator"/> receiving events from the <c>Direct</c> access store in the default namespace's
/// inbox of the <c>Ledger</c> application, which has the tenant namespace <see cref="Tenant"/>.
/// </summary>
public class an_access_event_translator : Specification
{
    /// <summary>
    /// The collecting store.
    /// </summary>
    protected static readonly EventStoreName Application = "Studio";

    /// <summary>
    /// The access store the events come from.
    /// </summary>
    protected static readonly EventStoreName Desk = "Direct";

    /// <summary>
    /// The tenant namespace the collecting store serves.
    /// </summary>
    protected static readonly EventStoreNamespaceName Tenant = AccessEvents.Tenant.Value;

    /// <summary>
    /// When the desk recorded its events.
    /// </summary>
    protected static readonly DateTimeOffset Occurred = new(2026, 11, 2, 14, 15, 0, TimeSpan.Zero);

    protected IEventStore _inbox;
    protected EventStoreForTesting _tenantStore;
    protected IChronicleClient _client;
    protected RecordingLogger<AccessEventTranslator> _logger;
    protected AccessEventTranslator _translator;
    protected Cratis.Execution.CorrelationId _correlationId;
    protected EventSourceId _deskEventSource;
    protected IImmutableList<AppendedEvent> _recorded;

    /// <summary>
    /// Gets the namespaces the collecting store has.
    /// </summary>
    protected virtual IEnumerable<EventStoreNamespaceName> Namespaces => [EventStoreNamespaceName.Default, Tenant];

    void Establish()
    {
        _inbox = Substitute.For<IEventStore>();
        _inbox.GetNamespaces(Arg.Any<CancellationToken>()).Returns(_ => Namespaces);
        _tenantStore = new();
        _client = Substitute.For<IChronicleClient>();
        _client.GetEventStore(Application, Arg.Is<EventStoreNamespaceName?>(_ => _ == EventStoreNamespaceName.Default)).Returns(_inbox);
        _client.GetEventStore(Application, Arg.Is<EventStoreNamespaceName?>(_ => _ == Tenant)).Returns(_tenantStore);
        _logger = new();
        _translator = new(_client, _logger);
        _correlationId = Cratis.Execution.CorrelationId.New();
        _deskEventSource = EventSourceId.New();
    }

    void Destroy() => _tenantStore.Dispose();

    /// <summary>
    /// Creates the context of an event from the desk in the inbox. The desk's own event source id is not the user's.
    /// </summary>
    /// <param name="sequenceNumber">The sequence number of the event in the inbox.</param>
    /// <param name="occurred">When the desk recorded the event.</param>
    /// <returns>The context.</returns>
    protected EventContext InboxContext(ulong sequenceNumber = 0, DateTimeOffset? occurred = null) => EventContext.From(
        Application,
        EventStoreNamespaceName.Default,
        EventType.Unknown,
        EventSourceType.Default,
        _deskEventSource,
        EventStreamType.All,
        EventStreamId.Default,
        sequenceNumber,
        _correlationId,
        occurred ?? Occurred);

    /// <summary>
    /// Reads what is recorded on the user's right to give feedback in the tenant namespace.
    /// </summary>
    /// <param name="giver">The user.</param>
    /// <returns>An awaitable task.</returns>
    protected async Task ReadRecorded(Contracts.Access.FeedbackGiverIdentity giver) =>
        _recorded = await _tenantStore.EventLog.GetFromSequenceNumber(EventSequenceNumber.First, Contracts.Access.FeedbackGiverId.For(giver));
}
#endif
