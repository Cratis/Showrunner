// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.given;
using Cratis.Showrunner.Notes.Notebook;

namespace Cratis.Showrunner.Notes.Giving.GiveNoteOnContext.when_executing;

public class and_the_text_is_exactly_the_longest : a_note_command<GiveNoteOnContext>
{
    async Task Because() => _result = await _scenario.Execute(NoteEvents.OnContext(_noteId) with { Text = new string('x', NoteLimits.TextLength) });

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();
}
#endif
