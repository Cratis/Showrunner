// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Withdrawing.WithdrawNote.when_executing;

public class and_someone_else_gave_the_note : a_note_command<WithdrawNote>
{
    void Establish() => Given(NoteEvents.Given(new NoteGiver(Givers.SomeoneElse, "Grace Hopper")));

    async Task Because() => _result = await _scenario.Execute(new WithdrawNote(_noteId));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();
    [Fact] void should_have_validation_errors() => _result.ShouldHaveValidationErrors();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
