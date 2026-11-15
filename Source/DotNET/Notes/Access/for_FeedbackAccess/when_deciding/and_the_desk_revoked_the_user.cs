// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Arc.Validation;
using Cratis.Monads;
using Cratis.Showrunner.Notes.Access.for_FeedbackAccess.given;
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Access.for_FeedbackAccess.when_deciding;

/// <summary>
/// The desk granted and then revoked: the refusal is the same as for a user who was never granted, so it tells nothing about what the desk decided.
/// </summary>
public class and_the_desk_revoked_the_user : a_feedback_access
{
    Result<FeedbackPermit, ValidationResult> _permit;
    bool _may;

    async Task Because()
    {
        _givers.Grant(Givers.Identity);
        _givers.Revoke(Givers.Identity);
        _permit = await _access.Permit();
        _may = await _access.MayGiveFeedback();
    }

    [Fact] void should_refuse_the_command() => _permit.IsSuccess.ShouldBeFalse();
    [Fact] void should_tell_the_user_it_is_not_available() => ((ValidationResult)_permit).Message.ShouldEqual(FeedbackAccess.Refusal);
    [Fact] void should_not_be_advised_to_show_it() => _may.ShouldBeFalse();
}
#endif
