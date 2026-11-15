// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.Notes.Transport.given;

namespace Cratis.Showrunner.Notes.Transport.for_OutboxRelay.when_relaying;

/// <summary>
/// The marker of a staged application is a Notes contract like the notes: it is relayed right after its note.
/// </summary>
public class and_the_application_is_staged : an_outbox_relay
{
    async Task Because()
    {
        var noteId = NoteId.New();
        await Publish(noteId);
        await _eventStore.GetEventSequence(EventSequenceId.Outbox).Append(noteId, new NoteGivenOnStagedApplication("fjord-logistics"));
        await _relay.Relay(_eventStore);
    }

    [Fact] void should_send_the_note_and_the_marker() => _collector.AllNotes.Count.ShouldEqual(2);
    [Fact] void should_send_the_marker_after_the_note() => _collector.AllNotes[1].SequenceNumber.ShouldEqual(1ul);
}
#endif
