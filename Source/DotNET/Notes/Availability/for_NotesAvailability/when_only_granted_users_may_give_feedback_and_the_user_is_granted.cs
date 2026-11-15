// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Showrunner.Notes.Access;
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Availability.for_NotesAvailability;

/// <summary>
/// Only granted users may give feedback and the desk granted the user: the button shows.
/// </summary>
public class when_only_granted_users_may_give_feedback_and_the_user_is_granted : a_query<NotesAvailability>
{
    protected override FeedbackAccessMode AccessMode => FeedbackAccessMode.GrantedUsers;

    void Establish()
    {
        _givers.Grant(Givers.Identity);
    }

    async Task Because() => _result = await _scenario.Perform(nameof(NotesAvailability.Availability));

    [Fact] void should_succeed() => _result.IsSuccess.ShouldBeTrue();
    [Fact] void should_be_on() => ((NotesAvailability)_result.Data).IsEnabled.ShouldBeTrue();
}
#endif
