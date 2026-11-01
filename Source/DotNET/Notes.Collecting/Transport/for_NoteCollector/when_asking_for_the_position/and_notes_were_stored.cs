// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.Transport.given;

namespace Cratis.Showrunner.Notes.Collecting.Transport.for_NoteCollector.when_asking_for_the_position;

public class and_notes_were_stored : a_note_collector
{
    (CollectOutcome Outcome, ulong? Position) _before;
    ulong? _afterTwoBatches;
    ulong? _afterAnOldBatch;
    ulong? _otherSource;
    ulong? _otherPublication;

    async Task Because()
    {
        _before = await _collector.Position(PublicationName, _publication, string.Empty);
        await _collector.Collect(PublicationName, _publication, string.Empty, [Relayed(Given(Tenant.Value), 3), Relayed(Given(Tenant.Value), 4, NoteId.New())]);
        await _collector.Collect(PublicationName, _publication, string.Empty, [Relayed(Given(Tenant.Value), 9, NoteId.New())]);
        _afterTwoBatches = (await _collector.Position(PublicationName, _publication, string.Empty)).Position;

        // A batch that was sent again late must not take the position back.
        await _collector.Collect(PublicationName, _publication, string.Empty, [Relayed(Given(Tenant.Value), 3)]);
        _afterAnOldBatch = (await _collector.Position(PublicationName, _publication, string.Empty)).Position;
        _otherSource = (await _collector.Position(PublicationName, _publication, "Default")).Position;
        _otherPublication = (await _collector.Position("pub-other", _publication, string.Empty)).Position;
    }

    [Fact] void should_have_no_position_before_anything_is_stored() => _before.Position.ShouldBeNull();
    [Fact] void should_report_the_highest_sequence_number_stored() => _afterTwoBatches.ShouldEqual(9ul);
    [Fact] void should_not_take_the_position_back() => _afterAnOldBatch.ShouldEqual(9ul);
    [Fact] void should_keep_a_position_per_source() => _otherSource.ShouldBeNull();
    [Fact] void should_keep_a_position_per_publication() => _otherPublication.ShouldBeNull();
}
#endif
