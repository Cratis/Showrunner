// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Access.given;

namespace Cratis.Showrunner.Notes.Access.for_FeedbackGiver.when_reducing;

/// <summary>
/// The desk granted, revoked and granted again: the latest decision stands and the user may give feedback.
/// </summary>
public class a_grant_after_a_revocation : a_feedback_giver
{
    FeedbackGiver? _state;

    async Task Because()
    {
        await Decided(AccessEvents.Granted(), AccessEvents.Revoked(), AccessEvents.Granted());
        _state = _scenario.Instance;
    }

    [Fact] void should_be_granted() => _state!.IsGranted.ShouldBeTrue();
    [Fact] void should_keep_when_the_desk_granted_again() => _state!.DecidedAt.ShouldEqual(At(2));
}
#endif
