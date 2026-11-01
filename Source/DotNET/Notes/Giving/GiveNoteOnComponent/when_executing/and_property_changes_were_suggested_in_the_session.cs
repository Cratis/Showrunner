// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Giving.GiveNoteOnComponent.when_executing;

/// <summary>
/// The text about a component shares its note id with the property changes suggested in the same picking session.
/// </summary>
public class and_property_changes_were_suggested_in_the_session : a_note_command<GiveNoteOnComponent>
{
    void Establish() => Given(NoteEvents.Suggested());

    async Task Because() => _result = await _scenario.Execute(NoteEvents.OnComponent(_noteId));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();
}
#endif
