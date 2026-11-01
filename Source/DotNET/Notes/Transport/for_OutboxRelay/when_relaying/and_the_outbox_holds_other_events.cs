// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Transport.given;

namespace Cratis.Showrunner.Notes.Transport.for_OutboxRelay.when_relaying;

/// <summary>
/// The outbox is shared with other parts; only the Notes contracts leave the application through Notes.
/// </summary>
public class and_the_outbox_holds_other_events : an_outbox_relay
{
    async Task Because()
    {
        await PublishSomethingElse(NoteId.New());
        await Publish(NoteId.New());
        await _relay.Relay(_eventStore);
    }

    [Fact] void should_send_only_the_note() => _collector.AllNotes.Count.ShouldEqual(1);
    [Fact] void should_keep_the_position_in_the_outbox_of_the_note() => _collector.AllNotes[0].SequenceNumber.ShouldEqual(1ul);
}
#endif
