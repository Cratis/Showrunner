// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.Testing.Events;
using Cratis.Showrunner.Notes.given;
using Cratis.Showrunner.Notes.Publishing;
using Cratis.Showrunner.Usher.Contracts.KnownIssues;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Transport.given;

/// <summary>
/// An <see cref="OutboxRelay"/> over the outbox of the application's event store.
/// </summary>
public class an_outbox_relay : Specification
{
    protected EventStoreForTesting _eventStore;
    protected RecordingCollector _collector;
    protected OutboxRelay _relay;
    protected NotesOptions _options;

    void Establish()
    {
        _eventStore = new();
        _collector = new();
        _options = new() { Transport = NotesTransport.Http };
        _relay = new(_collector, Options.Create(_options));
    }

    void Destroy() => _eventStore.Dispose();

    /// <summary>
    /// Publishes a note about the context to the outbox.
    /// </summary>
    /// <param name="noteId">The note.</param>
    /// <returns>An awaitable task.</returns>
    protected Task Publish(NoteId noteId) =>
        _eventStore.GetEventSequence(EventSequenceId.Outbox).Append(
            noteId,
            PublicEvents.For(NoteEvents.Given(), "fjord-logistics"),
            correlationId: Cratis.Execution.CorrelationId.New(),
            subject: Identity.NoteGivers.For(Givers.UserId));

    /// <summary>
    /// Appends something other than a Notes contract to the outbox, as another part would.
    /// </summary>
    /// <param name="noteId">The event source.</param>
    /// <returns>An awaitable task.</returns>
    protected Task PublishSomethingElse(NoteId noteId) =>
        _eventStore.GetEventSequence(EventSequenceId.Outbox).Append(noteId, new IssueUnlinkedFromKnownIssue(new("fjord-logistics"), new(Guid.NewGuid())));
}
#endif
