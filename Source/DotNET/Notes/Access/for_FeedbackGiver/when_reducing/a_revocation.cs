// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Access.given;

namespace Cratis.Showrunner.Notes.Access.for_FeedbackGiver.when_reducing;

/// <summary>
/// The desk granted the right and then revoked it: the user may not give feedback.
/// </summary>
public class a_revocation : a_feedback_giver
{
    FeedbackGiver? _state;

    async Task Because()
    {
        await Decided(AccessEvents.Granted(), AccessEvents.Revoked());
        _state = _scenario.Instance;
    }

    [Fact] void should_not_be_granted() => _state!.IsGranted.ShouldBeFalse();
    [Fact] void should_keep_when_the_desk_revoked() => _state!.DecidedAt.ShouldEqual(At(1));
}
#endif
