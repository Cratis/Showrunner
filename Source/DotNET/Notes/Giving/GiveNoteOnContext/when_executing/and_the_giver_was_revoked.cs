// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Access;
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Giving.GiveNoteOnContext.when_executing;

/// <summary>
/// Only granted users may give feedback, and the desk granted the giver the right and then revoked it: the refusal is the same as for a giver who was never granted, so it reveals nothing about the desk's decisions.
/// </summary>
public class and_the_giver_was_revoked : a_note_command<GiveNoteOnContext>
{
    protected override FeedbackAccessMode AccessMode => FeedbackAccessMode.GrantedUsers;

    void Establish()
    {
        _givers.Grant(Givers.Identity);
        _givers.Revoke(Givers.Identity);
    }

    async Task Because() => _result = await _scenario.Execute(NoteEvents.OnContext(_noteId));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();
    [Fact] void should_have_validation_errors() => _result.ShouldHaveValidationErrors();
    [Fact] void should_say_nothing_but_that_it_is_not_available() => _result.ValidationResults.Select(_ => _.Message).ShouldContainOnly(FeedbackAccess.Refusal);
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
