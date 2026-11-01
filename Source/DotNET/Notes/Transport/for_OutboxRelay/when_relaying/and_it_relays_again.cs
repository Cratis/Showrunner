// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Transport.given;

namespace Cratis.Showrunner.Notes.Transport.for_OutboxRelay.when_relaying;

public class and_it_relays_again : an_outbox_relay
{
    int _again;

    async Task Because()
    {
        await Publish(NoteId.New());
        await _relay.Relay(_eventStore);
        await Publish(NoteId.New());
        _again = await _relay.Relay(_eventStore);
    }

    [Fact] void should_not_ask_the_position_a_second_time() => _collector.PositionAsked.ShouldEqual(1);
    [Fact] void should_send_only_what_is_new() => _again.ShouldEqual(1);
    [Fact] void should_not_send_a_note_twice() => _collector.AllNotes.Select(_ => _.SequenceNumber).ShouldContainOnly(0ul, 1ul);
}
#endif
