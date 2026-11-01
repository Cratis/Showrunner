// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Collections.Immutable;
using System.Text.Json;
using Cratis.Chronicle;
using Cratis.Chronicle.Events;
using Cratis.Chronicle.Testing.Events;
using Cratis.Showrunner.Notes.Collecting.given;
using Cratis.Showrunner.Notes.Collecting.Receiving;
using Cratis.Showrunner.Notes.Contracts.Transport;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Collecting.Transport.given;

/// <summary>
/// A <see cref="NoteCollector"/> for the publication <see cref="PublicationName"/> of the application <c>Ledger</c>,
/// whose tenant namespace in the collecting store is <see cref="Tenant"/>.
/// </summary>
public class a_note_collector : Specification
{
    /// <summary>
    /// The publication.
    /// </summary>
    protected static readonly PublicationId PublicationName = "pub-7f3a";

    /// <summary>
    /// The tenant namespace of the collecting store.
    /// </summary>
    protected static readonly EventStoreNamespaceName Tenant = "fjord-logistics";

    /// <summary>
    /// When the application recorded its events.
    /// </summary>
    protected static readonly DateTimeOffset Occurred = new(2026, 10, 26, 9, 30, 0, TimeSpan.Zero);

    protected EventStoreForTesting _tenantStore;
    protected IChronicleClient _client;
    protected RecordingLogger<NoteCollector> _logger;
    protected RecordingLogger<AppEventTranslator> _translatorLogger;
    protected NoteCollector _collector;
    protected NotesPublication _publication;
    protected NoteId _noteId;

    /// <summary>
    /// Gets the namespaces the collecting store has.
    /// </summary>
    protected virtual IEnumerable<EventStoreNamespaceName> Namespaces => [EventStoreNamespaceName.Default, Tenant];

    void Establish()
    {
        var root = Substitute.For<IEventStore>();
        root.GetNamespaces(Arg.Any<CancellationToken>()).Returns(_ => Namespaces);
        _tenantStore = new();
        _client = Substitute.For<IChronicleClient>();
        _client.GetEventStore(Arg.Any<EventStoreName>(), Arg.Is<EventStoreNamespaceName?>(_ => _ == EventStoreNamespaceName.Default)).Returns(root);
        _client.GetEventStore(Arg.Any<EventStoreName>(), Arg.Is<EventStoreNamespaceName?>(_ => _ == Tenant)).Returns(_tenantStore);
        _logger = new();
        _translatorLogger = new();
        _collector = new(
            _client,
            Options.Create(new ChronicleClientOptions { EventStore = "Studio" }),
            new AppEventTranslator(_client, _translatorLogger),
            _logger);
        _publication = new(Tenant.Value, "Ledger");
        _noteId = NoteId.New();
    }

    void Destroy() => _tenantStore.Dispose();

    /// <summary>
    /// Creates the note about the context an application relays.
    /// </summary>
    /// <param name="tenant">The tenant the note names.</param>
    /// <returns>The public event.</returns>
    protected static NoteOnContextGiven Given(string tenant) => new(
        tenant,
        ReceivedNotes.Application,
        ReceivedNotes.Giver,
        ReceivedNotes.Context,
        NoteKind.Problem,
        "The invoice is not sent.");

    /// <summary>
    /// Wraps a public event as the application relays it.
    /// </summary>
    /// <param name="publicEvent">The public event.</param>
    /// <param name="sequenceNumber">Its sequence number in the application's outbox.</param>
    /// <param name="noteId">The note, by default the one of the specification.</param>
    /// <returns>The relayed note.</returns>
    protected PublishedNoteEvent Relayed(object publicEvent, ulong sequenceNumber, NoteId? noteId = null)
    {
        var type = publicEvent.GetType().GetEventType();
        return new(
            sequenceNumber,
            new(type.Id.Value, type.Generation.Value),
            Occurred,
            (noteId ?? _noteId).Value.ToString(),
            JsonSerializer.SerializeToElement(publicEvent, publicEvent.GetType(), NotesTransportJson.Options),
            new Guid(0xaaaaaaaa, 0xbbbb, 0xcccc, 0xdd, 0xdd, 0xee, 0xee, 0xee, 0xee, 0xee, 0xee),
            "notes-giver-abc");
    }

    /// <summary>
    /// Reads what is recorded on a note in the tenant namespace.
    /// </summary>
    /// <param name="noteId">The note, by default the one of the specification.</param>
    /// <returns>The recorded events.</returns>
    protected async Task<IImmutableList<AppendedEvent>> Recorded(NoteId? noteId = null) =>
        await _tenantStore.EventLog.GetFromSequenceNumber(EventSequenceNumber.First, noteId ?? _noteId);

    /// <summary>
    /// Reads everything recorded in the tenant namespace.
    /// </summary>
    /// <returns>The recorded events.</returns>
    protected async Task<IImmutableList<AppendedEvent>> RecordedAnywhere() =>
        await _tenantStore.EventLog.GetFromSequenceNumber(EventSequenceNumber.First);
}
#endif
