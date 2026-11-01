// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Giving.SuggestComponentPropertyChange.when_executing;

public class and_no_property_is_named : a_note_command<SuggestComponentPropertyChange>
{
    async Task Because() => _result = await _scenario.Execute(NoteEvents.Suggestion(_noteId) with { Property = ComponentProperty.NotSet });

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
