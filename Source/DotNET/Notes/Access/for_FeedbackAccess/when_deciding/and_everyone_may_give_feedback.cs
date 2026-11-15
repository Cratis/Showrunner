// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Arc.Validation;
using Cratis.Monads;
using Cratis.Showrunner.Notes.Access.for_FeedbackAccess.given;
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Access.for_FeedbackAccess.when_deciding;

/// <summary>
/// Everyone may give feedback, as before access could be controlled: a signed-in user is permitted without the desk deciding anything, and nothing is read.
/// </summary>
public class and_everyone_may_give_feedback : a_feedback_access
{
    protected override FeedbackAccessMode AccessMode => FeedbackAccessMode.Everyone;

    Result<FeedbackPermit, ValidationResult> _permit;
    bool _may;

    async Task Because()
    {
        _permit = await _access.Permit();
        _may = await _access.MayGiveFeedback();
    }

    [Fact] void should_permit_the_command() => _permit.IsSuccess.ShouldBeTrue();
    [Fact] void should_give_the_note_as_the_signed_in_user() => ((FeedbackPermit)_permit).Giver.ShouldEqual(Givers.Giver);
    [Fact] void should_not_read_what_the_desk_decided() => (_givers.DecisionReads + _givers.PlainReads).ShouldEqual(0);
    [Fact] void should_be_advised_to_show_it() => _may.ShouldBeTrue();
}
#endif
