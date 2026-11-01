// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.given;

namespace Cratis.Showrunner.Notes.Collecting.Responding.DeclineNote.when_executing;

public class and_the_note_was_never_received : a_collecting_command<DeclineNote>
{
    async Task Because() => _result = await _scenario.Execute(new DeclineNote(_noteId, "Out of scope."));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();
    [Fact] void should_have_validation_errors() => _result.ShouldHaveValidationErrors();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
