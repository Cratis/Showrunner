// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.given;

namespace Cratis.Showrunner.Notes.Collecting.Responding.AcknowledgeNote.when_executing;

public class and_the_note_is_open : a_collecting_command<AcknowledgeNote>
{
    void Establish() => Given(ReceivedNotes.OnContext);

    async Task Because() => _result = await _scenario.Execute(new AcknowledgeNote(_noteId));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_the_fact() =>
        await _scenario.ShouldHaveAppendedEvent<AcknowledgeNote, OperatorAcknowledgedNote>(_noteId);
}
#endif
