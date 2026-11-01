// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.Transport.given;

namespace Cratis.Showrunner.Notes.Collecting.Transport.for_NoteCollector.when_collecting;

public class and_the_event_is_not_one_an_application_sends : a_note_collector
{
    CollectOutcome _outcome;
    ulong? _position;

    async Task Because()
    {
        var response = Relayed(new NoteAcknowledged(Tenant.Value), 6);
        _outcome = await _collector.Collect(PublicationName, _publication, string.Empty, [response]);
        _position = (await _collector.Position(PublicationName, _publication, string.Empty)).Position;
    }

    [Fact] void should_carry_on() => _outcome.ShouldEqual(CollectOutcome.Stored);
    [Fact] async Task should_record_no_note() => (await RecordedAnywhere()).Any(_ => _.Content is Receiving.NoteWithdrawalReceived).ShouldBeFalse();
    [Fact] void should_still_pass_it_in_the_position() => _position.ShouldEqual(6ul);
}
#endif
