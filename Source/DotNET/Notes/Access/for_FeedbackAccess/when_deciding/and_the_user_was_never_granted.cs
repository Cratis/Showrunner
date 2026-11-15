// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Arc.Validation;
using Cratis.Monads;
using Cratis.Showrunner.Notes.Access.for_FeedbackAccess.given;
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Access.for_FeedbackAccess.when_deciding;

/// <summary>
/// The desk never decided anything about the user: they may not give feedback.
/// </summary>
public class and_the_user_was_never_granted : a_feedback_access
{
    Result<FeedbackPermit, ValidationResult> _permit;
    bool _may;

    async Task Because()
    {
        _permit = await _access.Permit();
        _may = await _access.MayGiveFeedback();
    }

    [Fact] void should_refuse_the_command() => _permit.IsSuccess.ShouldBeFalse();
    [Fact] void should_tell_the_user_it_is_not_available() => ((ValidationResult)_permit).Message.ShouldEqual(FeedbackAccess.Refusal);
    [Fact] void should_not_be_advised_to_show_it() => _may.ShouldBeFalse();
}
#endif
