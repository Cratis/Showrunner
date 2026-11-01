// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Transport.given;

namespace Cratis.Showrunner.Notes.Transport.for_OutboxRelay.when_relaying;

/// <summary>
/// After a failure the application asks the collector what it stored instead of trusting what it sent.
/// </summary>
public class and_the_collector_fails : an_outbox_relay
{
    Exception _failure;

    async Task Because()
    {
        await Publish(NoteId.New());
        await _relay.Relay(_eventStore);
        await Publish(NoteId.New());
        _collector.FailWith = new CollectorRequestFailed(System.Net.HttpStatusCode.InternalServerError);
        _failure = await Record.ExceptionAsync(() => _relay.Relay(_eventStore));
        _collector.FailWith = null;
        _collector.Position = 0;
        await _relay.Relay(_eventStore);
    }

    [Fact] void should_report_the_failure() => _failure.ShouldBeOfExactType<CollectorRequestFailed>();
    [Fact] void should_ask_for_the_position_again() => _collector.PositionAsked.ShouldEqual(2);
    [Fact] void should_send_the_note_after_the_stored_position() => _collector.AllNotes.Last().SequenceNumber.ShouldEqual(1ul);
}
#endif
