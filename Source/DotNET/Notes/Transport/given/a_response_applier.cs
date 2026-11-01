// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Chronicle.Testing.Events;
using Cratis.Showrunner.Notes.Contracts.Transport;
using Cratis.Showrunner.Notes.given;
using Cratis.Showrunner.Notes.Tenancy;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Transport.given;

/// <summary>
/// A <see cref="ResponseApplier"/> over an application with the tenant namespace <see cref="Tenant"/>, in which the
/// giver gave a note.
/// </summary>
public class a_response_applier : Specification
{
    /// <summary>
    /// The application's event store.
    /// </summary>
    protected static readonly EventStoreName Application = "Ledger";

    /// <summary>
    /// The tenant namespace the note was given in.
    /// </summary>
    protected static readonly EventStoreNamespaceName Tenant = "fjord-logistics";

    /// <summary>
    /// When the receiving side answered.
    /// </summary>
    protected static readonly DateTimeOffset Occurred = new(2026, 10, 26, 14, 15, 0, TimeSpan.Zero);

    protected IEventStore _root;
    protected EventStoreForTesting _tenantStore;
    protected IChronicleClient _client;
    protected RecordingCollector _collector;
    protected RecordingLogger<ResponseApplier> _logger;
    protected NoteId _noteId;
    protected ResponseApplier _applier;

    void Establish()
    {
        _root = Substitute.For<IEventStore>();
        _root.GetNamespaces(Arg.Any<CancellationToken>()).Returns(_ => [EventStoreNamespaceName.Default, Tenant]);
        _tenantStore = new();
        _client = Substitute.For<IChronicleClient>();
        _client.GetEventStore(Application, Arg.Is<EventStoreNamespaceName?>(_ => _ == EventStoreNamespaceName.Default)).Returns(_root);
        _client.GetEventStore(Application, Arg.Is<EventStoreNamespaceName?>(_ => _ == Tenant)).Returns(_tenantStore);
        _collector = new();
        _logger = new();
        _noteId = NoteId.New();
        _applier = Create();
    }

    void Destroy() => _tenantStore.Dispose();

    /// <summary>
    /// Creates an applier as a restarted application would, with no memory of the answers it applied.
    /// </summary>
    /// <returns>The applier.</returns>
    protected ResponseApplier Create() => new(
        _collector,
        new TenantEventStores(_client, Options.Create(new ChronicleClientOptions { EventStore = Application })),
        Options.Create(new NotesOptions { Transport = NotesTransport.Http, PublicationId = "pub-7f3a" }),
        _logger);

    /// <summary>
    /// Lets the giver give the note in the tenant namespace.
    /// </summary>
    /// <returns>An awaitable task.</returns>
    protected Task Given() => _tenantStore.EventLog.Append(_noteId, NoteEvents.Given());

    /// <summary>
    /// Creates an answer.
    /// </summary>
    /// <param name="cursor">Its cursor.</param>
    /// <param name="kind">What was done.</param>
    /// <param name="version">The version, for an addressed note.</param>
    /// <param name="reason">The reason, for a declined note.</param>
    /// <returns>The answer.</returns>
    protected NoteResponse Answer(ulong cursor, NoteResponseKind kind, string? version = null, string? reason = null) =>
        new(cursor, _noteId.Value, kind, Occurred, version, reason);

    /// <summary>
    /// Reads what is recorded on the note.
    /// </summary>
    /// <returns>The facts.</returns>
    protected async Task<IReadOnlyList<object>> Recorded() =>
        [.. (await _tenantStore.EventLog.GetFromSequenceNumber(Cratis.Chronicle.Events.EventSequenceNumber.First, _noteId)).Select(_ => _.Content)];
}
#endif
