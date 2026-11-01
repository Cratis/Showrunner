// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Collections.Immutable;
using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.Testing.Events;
using Cratis.Showrunner.Notes.Collecting.given;

namespace Cratis.Showrunner.Notes.Collecting.Receiving.for_AppEventTranslator.given;

/// <summary>
/// An <see cref="AppEventTranslator"/> receiving events from the <c>Ledger</c> application in the default namespace's
/// inbox of the collecting store, which has the tenant namespace <see cref="Tenant"/>.
/// </summary>
public class an_app_event_translator : Specification
{
    /// <summary>
    /// The collecting store.
    /// </summary>
    protected static readonly EventStoreName Collecting = "Studio";

    /// <summary>
    /// The application store the events come from.
    /// </summary>
    protected static readonly EventStoreName Application = "Ledger";

    /// <summary>
    /// The tenant namespace the notes are for.
    /// </summary>
    protected static readonly EventStoreNamespaceName Tenant = "fjord-logistics";

    /// <summary>
    /// When the application recorded its events.
    /// </summary>
    protected static readonly DateTimeOffset Occurred = new(2026, 10, 26, 9, 30, 0, TimeSpan.Zero);

    protected IEventStore _inbox;
    protected EventStoreForTesting _tenantStore;
    protected IChronicleClient _client;
    protected RecordingLogger<AppEventTranslator> _logger;
    protected AppEventTranslator _translator;
    protected NoteId _noteId;
    protected Cratis.Execution.CorrelationId _correlationId;
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
        _client.GetEventStore(Collecting, Arg.Is<EventStoreNamespaceName?>(_ => _ == EventStoreNamespaceName.Default)).Returns(_inbox);
        _client.GetEventStore(Collecting, Arg.Is<EventStoreNamespaceName?>(_ => _ == Tenant)).Returns(_tenantStore);
        _logger = new();
        _translator = new(_client, _logger);
        _noteId = NoteId.New();
        _correlationId = Cratis.Execution.CorrelationId.New();
    }

    void Destroy() => _tenantStore.Dispose();

    /// <summary>
    /// Creates the public event an application sends for a note about the context.
    /// </summary>
    /// <param name="tenant">The tenant it names.</param>
    /// <returns>The event.</returns>
    protected static NoteOnContextGiven Given(string tenant) => new(
        tenant,
        ReceivedNotes.Application,
        ReceivedNotes.Giver,
        ReceivedNotes.Context,
        NoteKind.Problem,
        "The invoice is not sent.");

    /// <summary>
    /// Creates the context of an event from the application in the inbox, with the giver as its subject.
    /// </summary>
    /// <param name="sequenceNumber">The sequence number of the event in the inbox.</param>
    /// <returns>The context.</returns>
    protected EventContext InboxContext(ulong sequenceNumber = 0) => EventContext.From(
        Collecting,
        EventStoreNamespaceName.Default,
        EventType.Unknown,
        EventSourceType.Default,
        _noteId,
        EventStreamType.All,
        EventStreamId.Default,
        sequenceNumber,
        _correlationId,
        Occurred) with { Subject = "notes-giver-abc" };

    /// <summary>
    /// Reads what is recorded on the note in the tenant namespace.
    /// </summary>
    /// <returns>An awaitable task.</returns>
    protected async Task ReadRecorded() =>
        _recorded = await _tenantStore.EventLog.GetFromSequenceNumber(EventSequenceNumber.First, _noteId);
}
#endif
