// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Giving.GiveNoteOnContext.when_executing;

/// <summary>
/// A retried command with the same note id does not give a second note.
/// </summary>
public class and_the_note_is_already_given : a_note_command<GiveNoteOnContext>
{
    void Establish() => Given(NoteEvents.Given());

    async Task Because() => _result = await _scenario.Execute(NoteEvents.OnContext(_noteId));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();
    [Fact] void should_have_validation_errors() => _result.ShouldHaveValidationErrors();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
