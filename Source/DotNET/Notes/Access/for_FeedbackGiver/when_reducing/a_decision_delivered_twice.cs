// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Access.given;

namespace Cratis.Showrunner.Notes.Access.for_FeedbackGiver.when_reducing;

/// <summary>
/// Chronicle can deliver a decision again; the state is the same.
/// </summary>
public class a_decision_delivered_twice : a_feedback_giver
{
    FeedbackGiver? _state;

    async Task Because()
    {
        await DecidedAt((AccessEvents.Granted(), At(3)), (AccessEvents.Granted(), At(3)));
        _state = _scenario.Instance;
    }

    [Fact] void should_be_granted() => _state!.IsGranted.ShouldBeTrue();
    [Fact] void should_keep_when_the_desk_decided() => _state!.DecidedAt.ShouldEqual(At(3));
}
#endif
