// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Contracts.Transport;
using Cratis.Showrunner.Notes.Receiving;
using Cratis.Showrunner.Notes.Transport.given;

namespace Cratis.Showrunner.Notes.Transport.for_ResponseApplier.when_applying;

/// <summary>
/// A note addressed, then declined, then addressed again ends addressed. Reading the first answer again after a restart
/// must not turn it back: the cursor, not the note's state, decides what was applied.
/// </summary>
public class and_the_application_restarts_after_the_note_changed_its_mind : a_response_applier
{
    IReadOnlyList<object> _recorded;

    async Task Because()
    {
        await Given();
        _collector.Answers.Add(Answer(1, NoteResponseKind.Addressed, version: "2.0.0"));
        _collector.Answers.Add(Answer(2, NoteResponseKind.Declined, reason: "Not now."));
        _collector.Answers.Add(Answer(3, NoteResponseKind.Addressed, version: "2.0.0"));
        await _applier.Apply();
        await Create().Apply();
        _recorded = await Recorded();
    }

    [Fact] void should_record_each_answer_once() => _recorded.Count(_ => _ is ReceiverAddressedNote or ReceiverDeclinedNote).ShouldEqual(3);
    [Fact] void should_end_where_the_last_answer_left_it() => _recorded.Last().ShouldBeOfExactType<ReceiverAddressedNote>();
}
#endif
