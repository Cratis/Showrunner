// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.given;

namespace Cratis.Showrunner.Notes.Collecting.Responding.AcknowledgeNote.when_executing;

public class and_the_note_is_already_acknowledged : a_collecting_command<AcknowledgeNote>
{
    void Establish() => Given(ReceivedNotes.OnContext, new OperatorAcknowledgedNote());

    async Task Because() => _result = await _scenario.Execute(new AcknowledgeNote(_noteId));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
