// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Showrunner.Notes.Access;
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Availability.for_NotesAvailability;

/// <summary>
/// Only granted users may give feedback and nobody is signed in: the question is still answered, with no.
/// </summary>
public class when_only_granted_users_may_give_feedback_and_nobody_is_signed_in : a_query<NotesAvailability>
{
    protected override FeedbackAccessMode AccessMode => FeedbackAccessMode.GrantedUsers;

    protected override ClaimsPrincipal Principal => Givers.Anonymous();

    void Establish()
    {
        _givers.Grant(Givers.Identity);
    }

    async Task Because() => _result = await _scenario.Perform(nameof(NotesAvailability.Availability));

    [Fact] void should_succeed() => _result.IsSuccess.ShouldBeTrue();
    [Fact] void should_be_off() => ((NotesAvailability)_result.Data).IsEnabled.ShouldBeFalse();
}
#endif
