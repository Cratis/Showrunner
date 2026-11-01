// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Contracts.Responses;
using Cratis.Showrunner.Notes.Contracts.Transport;
using Cratis.Showrunner.Notes.Receiving;
using Cratis.Showrunner.Notes.Transport.given;

namespace Cratis.Showrunner.Notes.Transport.for_ResponseApplier.when_applying;

/// <summary>
/// The facts are the ones the same-kernel transport records.
/// </summary>
public class and_each_kind_of_answer_arrives : a_response_applier
{
    IReadOnlyList<object> _recorded;

    async Task Because()
    {
        await Given();
        _collector.Answers.Add(Answer(1, NoteResponseKind.Acknowledged));
        _collector.Answers.Add(Answer(2, NoteResponseKind.Addressed, version: "2.0.0"));
        _collector.Answers.Add(Answer(3, NoteResponseKind.Declined, reason: "Out of scope."));
        await _applier.Apply();
        _recorded = await Recorded();
    }

    [Fact] void should_record_the_acknowledgement() => _recorded.OfType<ReceiverAcknowledgedNote>().Count().ShouldEqual(1);
    [Fact] void should_record_the_version_it_was_addressed_in() => _recorded.OfType<ReceiverAddressedNote>().Single().Version.ShouldEqual(new AddressedInVersion("2.0.0"));
    [Fact] void should_record_why_it_was_declined() => _recorded.OfType<ReceiverDeclinedNote>().Single().Reason.ShouldEqual(new DeclineReason("Out of scope."));
    [Fact] void should_move_the_cursor_to_the_last_answer() => _applier.Cursor.ShouldEqual(3ul);
}
#endif
