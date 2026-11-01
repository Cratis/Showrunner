// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Giving.GiveNoteOnContext.when_executing;

/// <summary>
/// The signed-in user gives a note about the context: the giver and the application come from the identity and the options.
/// </summary>
public class and_the_giver_is_signed_in : a_note_command<GiveNoteOnContext>
{
    async Task Because() => _result = await _scenario.Execute(NoteEvents.OnContext(_noteId));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_take_the_giver_from_the_signed_in_identity() =>
        await _scenario.ShouldHaveAppendedEvent<GiveNoteOnContext, GiverGaveNoteOnContext>(_noteId, e => e.Giver == Givers.Giver);

    [Fact]
    async Task should_name_the_application_from_the_options() =>
        await _scenario.ShouldHaveAppendedEvent<GiveNoteOnContext, GiverGaveNoteOnContext>(_noteId, e => e.Application == NoteEvents.Application);

    [Fact]
    async Task should_append_the_text_the_giver_wrote() =>
        await _scenario.ShouldHaveAppendedEvent<GiveNoteOnContext, GiverGaveNoteOnContext>(_noteId, e => e.Kind == NoteEvents.OnContext(_noteId).Kind && e.Text == NoteEvents.OnContext(_noteId).Text);
}
#endif
