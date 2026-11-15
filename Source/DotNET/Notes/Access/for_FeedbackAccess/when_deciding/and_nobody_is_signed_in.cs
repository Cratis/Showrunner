// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Arc.Validation;
using Cratis.Monads;
using Cratis.Showrunner.Notes.Access.for_FeedbackAccess.given;
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Access.for_FeedbackAccess.when_deciding;

/// <summary>
/// Nobody is signed in, so there is no one to permit, however many users the desk granted.
/// </summary>
public class and_nobody_is_signed_in : a_feedback_access
{
    protected override System.Security.Claims.ClaimsPrincipal Principal => Givers.Anonymous();

    Result<FeedbackPermit, ValidationResult> _permit;
    bool _may;

    async Task Because()
    {
        _givers.Grant(Givers.Identity);
        _permit = await _access.Permit();
        _may = await _access.MayGiveFeedback();
    }

    [Fact] void should_refuse_the_command() => _permit.IsSuccess.ShouldBeFalse();
    [Fact] void should_not_read_what_the_desk_decided() => _givers.DecisionReads.ShouldEqual(0);
    [Fact] void should_not_be_advised_to_show_it() => _may.ShouldBeFalse();
}
#endif
