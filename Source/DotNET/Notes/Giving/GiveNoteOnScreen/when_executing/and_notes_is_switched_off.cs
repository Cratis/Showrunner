// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Giving.GiveNoteOnScreen.when_executing;

/// <summary>
/// The host switched Notes off, so giving a note about the screen is refused.
/// </summary>
public class and_notes_is_switched_off : a_note_command<GiveNoteOnScreen>
{
    protected override bool Enabled => false;

    async Task Because() => _result = await _scenario.Execute(NoteEvents.OnScreen(_noteId));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();
    [Fact] void should_have_validation_errors() => _result.ShouldHaveValidationErrors();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
