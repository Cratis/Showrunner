// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.Transport.given;

namespace Cratis.Showrunner.Notes.Collecting.Transport.for_NoteCollector.when_collecting;

/// <summary>
/// Two applications have their own outbox, so the same sequence number is two different notes.
/// </summary>
public class and_the_same_sequence_number_arrives_from_another_publication : a_note_collector
{
    async Task Because()
    {
        await _collector.Collect(PublicationName, _publication, string.Empty, [Relayed(Given(Tenant.Value), 4)]);
        await _collector.Collect("pub-other", _publication, string.Empty, [Relayed(Given(Tenant.Value), 4, NoteId.New())]);
    }

    [Fact] async Task should_record_both() => (await RecordedAnywhere()).Count(_ => _.Content is not Transport.NoteSequenceCollected).ShouldEqual(2);
}
#endif
