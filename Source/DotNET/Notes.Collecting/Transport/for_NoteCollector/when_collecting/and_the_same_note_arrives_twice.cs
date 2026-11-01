// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.Transport.given;

namespace Cratis.Showrunner.Notes.Collecting.Transport.for_NoteCollector.when_collecting;

/// <summary>
/// A relay that was not told its batch arrived sends it again, possibly in a bigger batch.
/// </summary>
public class and_the_same_note_arrives_twice : a_note_collector
{
    async Task Because()
    {
        await _collector.Collect(PublicationName, _publication, string.Empty, [Relayed(Given(Tenant.Value), 4)]);
        await _collector.Collect(PublicationName, _publication, string.Empty, [Relayed(Given(Tenant.Value), 4)]);
        await _collector.Collect(PublicationName, _publication, string.Empty, [Relayed(Given(Tenant.Value), 4), Relayed(Given(Tenant.Value), 5, NoteId.New())]);
    }

    [Fact] async Task should_record_it_once() => (await Recorded()).Count.ShouldEqual(1);
    [Fact] async Task should_record_the_other_note() => (await RecordedAnywhere()).Count(_ => _.Content is not Transport.NoteSequenceCollected).ShouldEqual(2);
}
#endif
