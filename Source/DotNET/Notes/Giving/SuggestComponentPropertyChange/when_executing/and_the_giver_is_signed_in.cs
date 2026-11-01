// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Giving.SuggestComponentPropertyChange.when_executing;

/// <summary>
/// The signed-in user gives a property change: the giver and the application come from the identity and the options.
/// </summary>
public class and_the_giver_is_signed_in : a_note_command<SuggestComponentPropertyChange>
{
    async Task Because() => _result = await _scenario.Execute(NoteEvents.Suggestion(_noteId));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_take_the_giver_from_the_signed_in_identity() =>
        await _scenario.ShouldHaveAppendedEvent<SuggestComponentPropertyChange, GiverSuggestedComponentPropertyChange>(_noteId, e => e.Giver == Givers.Giver);

    [Fact]
    async Task should_name_the_application_from_the_options() =>
        await _scenario.ShouldHaveAppendedEvent<SuggestComponentPropertyChange, GiverSuggestedComponentPropertyChange>(_noteId, e => e.Application == NoteEvents.Application);
}
#endif
