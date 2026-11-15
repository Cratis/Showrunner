// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Arc.Validation;
using Cratis.Monads;
using Cratis.Showrunner.Notes.Access.for_FeedbackAccess.given;
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Access.for_FeedbackAccess.when_deciding;

/// <summary>
/// Everyone may give feedback but nobody is signed in: the command is refused as always, while the availability answer stays the same for everyone.
/// </summary>
public class and_everyone_may_give_feedback_but_nobody_is_signed_in : a_feedback_access
{
    protected override FeedbackAccessMode AccessMode => FeedbackAccessMode.Everyone;

    protected override System.Security.Claims.ClaimsPrincipal Principal => Givers.Anonymous();

    Result<FeedbackPermit, ValidationResult> _permit;
    bool _may;

    async Task Because()
    {
        _permit = await _access.Permit();
        _may = await _access.MayGiveFeedback();
    }

    [Fact] void should_refuse_the_command() => _permit.IsSuccess.ShouldBeFalse();
    [Fact] void should_still_be_advised_to_show_it() => _may.ShouldBeTrue();
}
#endif
