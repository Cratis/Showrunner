// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.Receiving;
using Cratis.Showrunner.Notes.Collecting.Transport.given;
using Cratis.Showrunner.Notes.Contracts.Transport;

namespace Cratis.Showrunner.Notes.Collecting.Transport.for_NoteCollector.when_collecting;

/// <summary>
/// An application relays a note and then the marker that it ran staged: both are recorded on the note, in order, and
/// relaying them again records nothing more.
/// </summary>
public class and_the_note_was_given_in_a_staged_session : a_note_collector
{
    CollectOutcome _outcome;
    int _recordedAfterTwice;

    async Task Because()
    {
        IReadOnlyList<PublishedNoteEvent> batch =
        [
            Relayed(Given(Tenant.Value), 0),
            Relayed(new NoteGivenOnStagedApplication(Tenant.Value), 1)
        ];
        _outcome = await _collector.Collect(PublicationName, _publication, string.Empty, batch);
        await _collector.Collect(PublicationName, _publication, string.Empty, batch);
        _recordedAfterTwice = (await Recorded()).Count;
    }

    [Fact] void should_store() => _outcome.ShouldEqual(CollectOutcome.Stored);
    [Fact] async Task should_record_the_marker_after_the_note() => (await Recorded())[1].Content.ShouldBeOfExactType<NoteOnStagedApplicationReceived>();
    [Fact] void should_record_nothing_more_the_second_time() => _recordedAfterTwice.ShouldEqual(2);
}
#endif
