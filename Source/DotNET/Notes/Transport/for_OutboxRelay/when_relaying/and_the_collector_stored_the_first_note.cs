// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Transport.given;

namespace Cratis.Showrunner.Notes.Transport.for_OutboxRelay.when_relaying;

/// <summary>
/// The application keeps no checkpoint of its own: it carries on after what the collector says it stored.
/// </summary>
public class and_the_collector_stored_the_first_note : an_outbox_relay
{
    int _sent;

    async Task Because()
    {
        _collector.Position = 0;
        await Publish(NoteId.New());
        await Publish(NoteId.New());
        _sent = await _relay.Relay(_eventStore);
    }

    [Fact] void should_send_only_what_is_after_the_position() => _collector.AllNotes.Select(_ => _.SequenceNumber).ShouldContainOnly(1ul);
    [Fact] void should_count_what_it_sent() => _sent.ShouldEqual(1);
}
#endif
