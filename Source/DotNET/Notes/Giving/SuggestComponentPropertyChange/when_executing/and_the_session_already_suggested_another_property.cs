// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Giving.SuggestComponentPropertyChange.when_executing;

/// <summary>
/// One command per property: the second suggestion in a picking session is recorded beside the first.
/// </summary>
public class and_the_session_already_suggested_another_property : a_note_command<SuggestComponentPropertyChange>
{
    void Establish() => Given(NoteEvents.Suggested());

    async Task Because() => _result = await _scenario.Execute(NoteEvents.Suggestion(_noteId) with { Property = "color" });

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_the_property() =>
        await _scenario.ShouldHaveAppendedEvent<SuggestComponentPropertyChange, GiverSuggestedComponentPropertyChange>(_noteId, e => e.Property == "color");
}
#endif
