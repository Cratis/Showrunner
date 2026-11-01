// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Giving.GiveNoteOnContext.when_executing;

public class and_someone_else_gave_a_note_with_the_id : a_note_command<GiveNoteOnContext>
{
    void Establish() => Given(NoteEvents.Given(new NoteGiver(Givers.SomeoneElse, "Grace Hopper")));

    async Task Because() => _result = await _scenario.Execute(NoteEvents.OnContext(_noteId));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
