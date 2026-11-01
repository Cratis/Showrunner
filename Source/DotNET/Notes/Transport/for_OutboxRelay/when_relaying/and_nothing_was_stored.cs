// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Transport.given;

namespace Cratis.Showrunner.Notes.Transport.for_OutboxRelay.when_relaying;

public class and_nothing_was_stored : an_outbox_relay
{
    NoteId _first;
    NoteId _second;
    int _sent;

    async Task Because()
    {
        _first = NoteId.New();
        _second = NoteId.New();
        await Publish(_first);
        await Publish(_second);
        _sent = await _relay.Relay(_eventStore);
    }

    [Fact] void should_ask_the_collector_for_its_position() => _collector.PositionAsked.ShouldEqual(1);
    [Fact] void should_send_every_note() => _sent.ShouldEqual(2);
    [Fact] void should_send_them_in_outbox_order() => _collector.AllNotes.Select(_ => _.SequenceNumber).ShouldContainOnly(0ul, 1ul);
    [Fact] void should_name_the_note() => _collector.AllNotes[0].EventSourceId.ShouldEqual(_first.Value.ToString());
    [Fact] void should_name_the_contract_event_type() => _collector.AllNotes[0].EventType.Id.ShouldEqual("NoteOnContextGiven");
    [Fact] void should_carry_the_content() => _collector.AllNotes[0].Content.GetProperty("text").GetString().ShouldEqual("The invoice is not sent.");
    [Fact] void should_carry_the_subject() => _collector.AllNotes[0].Subject.ShouldEqual(Identity.NoteGivers.For(given_giver()).Value);

    static Cratis.Showrunner.Notes.Contracts.People.GiverUserId given_giver() => Cratis.Showrunner.Notes.given.Givers.UserId;
}
#endif
