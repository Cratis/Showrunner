// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Giving.GiveNoteOnContext.when_executing;

public class and_the_kind_is_unknown : a_note_command<GiveNoteOnContext>
{
    async Task Because() => _result = await _scenario.Execute(NoteEvents.OnContext(_noteId) with { Kind = (NoteKind)42 });

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
