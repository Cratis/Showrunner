// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Arc.Validation;
using Cratis.Monads;
using Cratis.Showrunner.Notes.Access.for_FeedbackAccess.given;
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Access.for_FeedbackAccess.when_deciding;

/// <summary>
/// The desk granted the user the right: the command gets a permit for the signed-in giver, decided with a read that guards the append.
/// </summary>
public class and_the_desk_granted_the_user : a_feedback_access
{
    Result<FeedbackPermit, ValidationResult> _permit;
    bool _may;

    async Task Because()
    {
        _givers.Grant(Givers.Identity);
        _permit = await _access.Permit();
        _may = await _access.MayGiveFeedback();
    }

    [Fact] void should_permit_the_command() => _permit.IsSuccess.ShouldBeTrue();
    [Fact] void should_give_the_note_as_the_signed_in_user() => ((FeedbackPermit)_permit).Giver.ShouldEqual(Givers.Giver);
    [Fact] void should_decide_with_the_read_that_guards_the_append() => _givers.DecisionReads.ShouldEqual(1);
    [Fact] void should_be_advised_to_show_it() => _may.ShouldBeTrue();
}
#endif
