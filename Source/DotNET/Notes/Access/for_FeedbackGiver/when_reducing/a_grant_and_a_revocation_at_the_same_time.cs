// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Access.given;

namespace Cratis.Showrunner.Notes.Access.for_FeedbackGiver.when_reducing;

/// <summary>
/// The desk decided both at the same time: the revocation wins, whichever reaches the application first.
/// </summary>
public class a_grant_and_a_revocation_at_the_same_time : a_feedback_giver
{
    FeedbackGiver? _state;

    async Task Because()
    {
        await DecidedAt((AccessEvents.Granted(), At(3)), (AccessEvents.Revoked(), At(3)), (AccessEvents.Granted(), At(3)));
        _state = _scenario.Instance;
    }

    [Fact] void should_not_be_granted() => _state!.IsGranted.ShouldBeFalse();
}
#endif
