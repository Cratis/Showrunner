// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.Notes.Collecting.Receiving;
using Cratis.Showrunner.Notes.Collecting.Responding.AcknowledgeNote;
using Cratis.Showrunner.Notes.Collecting.Responding.AddressNote;
using Cratis.Showrunner.Notes.Collecting.Responding.DeclineNote;
using Cratis.Showrunner.Notes.Collecting.Tenancy;
using Cratis.Showrunner.Notes.Contracts.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Collecting.Transport;

/// <summary>
/// The collector's side of the HTTP transport: stores the notes an application relays, says how far it stored them and
/// hands the application the answers.
/// </summary>
/// <remarks>
/// <para>
/// The publication the caller authenticated as decides the namespace. The tenant a posted note names is never used to pick
/// one: it is compared with the publication's tenant, and a batch with a note that names another is rejected whole and
/// logged without its content.
/// </para>
/// <para>
/// A note is translated by <see cref="PrivateEvents.For"/> and recorded by <see cref="AppEventTranslator.Record"/>, the
/// same code the same-kernel transport uses, so both record the same private facts. It is recorded once per publication,
/// source and outbox sequence number: the idempotency key is made of the three. A batch is read and checked in full before
/// any note of it is recorded, and its position is recorded last, so a batch that fails halfway is sent again and the
/// notes it did record are found.
/// </para>
/// </remarks>
/// <param name="client">The Chronicle client, to reach the namespace of the tenant.</param>
/// <param name="options">The Chronicle client options, which name the collecting event store.</param>
/// <param name="translator">Records a fact of an application.</param>
/// <param name="logger">The logger.</param>
public class NoteCollector(IChronicleClient client, IOptions<ChronicleClientOptions> options, AppEventTranslator translator, ILogger<NoteCollector> logger)
{
    /// <summary>
    /// The most answers handed out in one call.
    /// </summary>
    public const int ResponsePageSize = 100;

    static readonly Dictionary<string, Type> _applicationEvents = PrivateEvents.FromApplications
        .ToDictionary(type => type.GetEventType().Id.Value);

    static readonly Type[] _operatorFacts = [typeof(OperatorAcknowledgedNote), typeof(OperatorAddressedNote), typeof(OperatorDeclinedNote)];

    /// <summary>
    /// Derives the idempotency key of a relayed note.
    /// </summary>
    /// <param name="publicationId">The publication.</param>
    /// <param name="source">The event log of the application the note was relayed from.</param>
    /// <param name="sequenceNumber">The sequence number of the note in the outbox of the application.</param>
    /// <returns>The key.</returns>
    public static IdempotencyKey KeyFor(PublicationId publicationId, string source, ulong sequenceNumber) =>
        new($"notes-collecting-http-{publicationId.Value}-{source}-{sequenceNumber}");

    /// <summary>
    /// Stores the notes an application relayed.
    /// </summary>
    /// <param name="publicationId">The publication the caller authenticated as.</param>
    /// <param name="publication">What is known about the publication.</param>
    /// <param name="source">The event log of the application the notes were relayed from; empty for the default.</param>
    /// <param name="notes">The notes, in outbox order.</param>
    /// <returns>How the batch was received.</returns>
    public async Task<CollectOutcome> Collect(PublicationId publicationId, NotesPublication publication, string source, IReadOnlyList<PublishedNoteEvent> notes)
    {
        var translated = new List<(PublishedNoteEvent Note, string EventType, object Fact)>();
        foreach (var note in notes.OrderBy(_ => _.SequenceNumber))
        {
            if (!_applicationEvents.TryGetValue(note.EventType.Id, out var type) || note.EventType.Generation != type.GetEventType().Generation.Value)
            {
                logger.NotAnApplicationEvent(publicationId, note.SequenceNumber, note.EventType.Id);
                continue;
            }

            if (Read(note, type) is not { } content || !Guid.TryParse(note.EventSourceId, out _) || PrivateEvents.For(content, publication.AppStore) is not { } fact)
            {
                logger.Unreadable(publicationId, note.SequenceNumber);
                return CollectOutcome.Invalid;
            }

            if (!string.Equals(fact.TenantId.Value, publication.TenantId.Value, StringComparison.Ordinal))
            {
                logger.TenantMismatch(publicationId, note.SequenceNumber);
                return CollectOutcome.TenantMismatch;
            }

            translated.Add((note, type.Name, fact.Fact));
        }

        if (await Tenant(publication) is not var (tenant, @namespace))
        {
            logger.UnknownTenant(publicationId);
            return CollectOutcome.UnknownTenant;
        }

        var appStore = new EventStoreName(publication.AppStore.Value);
        foreach (var (note, eventType, fact) in translated)
        {
            await translator.Record(
                tenant,
                @namespace,
                publication.TenantId,
                fact,
                note.EventSourceId,
                KeyFor(publicationId, source, note.SequenceNumber),
                note.CorrelationId is { } correlation ? new(correlation) : Cratis.Execution.CorrelationId.New(),
                note.Occurred,
                string.IsNullOrEmpty(note.Subject) ? Subject.NotSet : new(note.Subject),
                appStore,
                eventType);
        }

        if (notes.Count > 0)
        {
            var highest = notes.Max(_ => _.SequenceNumber);
            await Advance(tenant, publicationId, source, highest);
            logger.Stored(publicationId, translated.Count, highest);
        }

        return CollectOutcome.Stored;
    }

    /// <summary>
    /// Gets the highest outbox sequence number stored for a publication and source.
    /// </summary>
    /// <param name="publicationId">The publication.</param>
    /// <param name="publication">What is known about the publication.</param>
    /// <param name="source">The event log of the application; empty for the default.</param>
    /// <returns>The sequence number, or <see langword="null"/> when none was stored; a <see cref="CollectOutcome"/> other than stored when the tenant has no namespace.</returns>
    public async Task<(CollectOutcome Outcome, ulong? Position)> Position(PublicationId publicationId, NotesPublication publication, string source)
    {
        if (await Tenant(publication) is not var (tenant, _))
        {
            logger.UnknownTenant(publicationId);
            return (CollectOutcome.UnknownTenant, null);
        }

        var recorded = await tenant.EventLog.GetForEventSourceIdAndEventTypes(PositionOf(publicationId, source), [tenant.EventTypes.GetEventTypeFor(typeof(NoteSequenceCollected))]);
        var positions = recorded.Select(_ => ((NoteSequenceCollected)_.Content).SequenceNumber).ToArray();
        return (CollectOutcome.Stored, positions.Length == 0 ? null : positions.Max());
    }

    /// <summary>
    /// Gets what the builder did with the notes of a publication, after a cursor.
    /// </summary>
    /// <param name="publicationId">The publication.</param>
    /// <param name="publication">What is known about the publication.</param>
    /// <param name="after">The cursor of the last answer the caller saw; zero for all.</param>
    /// <returns>The answers in cursor order, at most <see cref="ResponsePageSize"/>; a <see cref="CollectOutcome"/> other than stored when the tenant has no namespace.</returns>
    public async Task<(CollectOutcome Outcome, IReadOnlyList<NoteResponse> Responses)> Responses(PublicationId publicationId, NotesPublication publication, ulong after)
    {
        if (await Tenant(publication) is not var (tenant, _))
        {
            logger.UnknownTenant(publicationId);
            return (CollectOutcome.UnknownTenant, []);
        }

        var types = _operatorFacts.Select(tenant.EventTypes.GetEventTypeFor).ToArray();

        // A cursor is the sequence number of the answer plus one, so the answers after a cursor start at that sequence number.
        var answers = await tenant.EventLog.GetFromSequenceNumber(new EventSequenceNumber(after), filterEventTypes: types);
        var belongs = new Dictionary<string, bool>();
        var responses = new List<NoteResponse>();
        foreach (var answer in answers.OrderBy(_ => _.Context.SequenceNumber.Value))
        {
            if (responses.Count == ResponsePageSize)
            {
                break;
            }

            var noteId = answer.Context.EventSourceId.Value;
            if (!belongs.TryGetValue(noteId, out var ours))
            {
                ours = await BelongsTo(tenant, answer.Context.EventSourceId, publicationId);
                belongs[noteId] = ours;
            }

            if (ours && Guid.TryParse(noteId, out var guid))
            {
                responses.Add(Describe(answer, guid));
            }
        }

        return (CollectOutcome.Stored, responses);
    }

    static EventSourceId PositionOf(PublicationId publicationId, string source) => new($"{publicationId.Value}|{source}");

    static object? Read(PublishedNoteEvent note, Type type)
    {
        try
        {
            return note.Content.Deserialize(type, NotesTransportJson.Options);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    static NoteResponse Describe(AppendedEvent answer, Guid noteId)
    {
        var cursor = answer.Context.SequenceNumber.Value + 1;
        return answer.Content switch
        {
            OperatorAddressedNote addressed => new(cursor, noteId, NoteResponseKind.Addressed, answer.Context.Occurred, addressed.Version.Value),
            OperatorDeclinedNote declined => new(cursor, noteId, NoteResponseKind.Declined, answer.Context.Occurred, null, declined.Reason.Value),
            _ => new(cursor, noteId, NoteResponseKind.Acknowledged, answer.Context.Occurred)
        };
    }

    static async Task<bool> BelongsTo(IEventStore tenant, EventSourceId noteId, PublicationId publicationId)
    {
        Type[] received =
        [
            typeof(NoteOnContextReceived),
            typeof(NoteOnSlicesReceived),
            typeof(NoteOnScreenReceived),
            typeof(NoteOnComponentReceived),
            typeof(ComponentPropertyChangeReceived)
        ];
        var openers = received.Select(tenant.EventTypes.GetEventTypeFor).ToArray();
        var opened = await tenant.EventLog.GetForEventSourceIdAndEventTypes(noteId, openers);
        return opened.Count > 0 && ApplicationOf(opened[0].Content)?.PublicationId == publicationId;
    }

    static PublishedApplication? ApplicationOf(object received) => received switch
    {
        NoteOnContextReceived note => note.Application,
        NoteOnSlicesReceived note => note.Application,
        NoteOnScreenReceived note => note.Application,
        NoteOnComponentReceived note => note.Application,
        ComponentPropertyChangeReceived note => note.Application,
        _ => null
    };

    async Task Advance(IEventStore tenant, PublicationId publicationId, string source, ulong highest)
    {
        var eventSourceId = PositionOf(publicationId, source);
        var recorded = await tenant.EventLog.GetForEventSourceIdAndEventTypes(eventSourceId, [tenant.EventTypes.GetEventTypeFor(typeof(NoteSequenceCollected))]);
        if (recorded.Any(_ => ((NoteSequenceCollected)_.Content).SequenceNumber >= highest))
        {
            return;
        }

        var result = await tenant.EventLog.AppendOnce(
            eventSourceId,
            new NoteSequenceCollected(publicationId, source, highest),
            new($"notes-collecting-position-{publicationId.Value}-{source}-{highest}"));
        if (!result.IsSuccess && !result.WasAlreadyAppended)
        {
            throw new NoteFactNotRecorded(typeof(NoteSequenceCollected), eventSourceId, tenant.Namespace, result);
        }
    }

    async Task<(IEventStore Store, EventStoreNamespaceName Namespace)?> Tenant(NotesPublication publication)
    {
        var name = options.Value.EventStore;
        var root = await client.GetEventStore(name, EventStoreNamespaceName.Default);
        var found = TenantNamespaces.Find(await root.GetNamespaces(), publication.TenantId);
        if (found.Namespace is not { } @namespace)
        {
            return null;
        }

        return (await client.GetEventStore(name, @namespace), @namespace);
    }
}
