// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.Transport.given;

namespace Cratis.Showrunner.Notes.Collecting.Transport.for_NoteCollector.when_collecting;

public class and_a_note_that_was_never_received_is_withdrawn : a_note_collector
{
    async Task Because() => await _collector.Collect(PublicationName, _publication, string.Empty, [Relayed(new NoteWithdrawn(Tenant.Value), 4)]);

    [Fact] async Task should_record_nothing_on_the_note() => (await Recorded()).Count.ShouldEqual(0);
}
#endif
