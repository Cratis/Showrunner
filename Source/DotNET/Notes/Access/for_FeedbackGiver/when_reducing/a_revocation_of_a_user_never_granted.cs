// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Access.given;

namespace Cratis.Showrunner.Notes.Access.for_FeedbackGiver.when_reducing;

/// <summary>
/// The desk revoked a right that was never granted: the user still may not give feedback.
/// </summary>
public class a_revocation_of_a_user_never_granted : a_feedback_giver
{
    FeedbackGiver? _state;

    async Task Because()
    {
        await Decided(AccessEvents.Revoked());
        _state = _scenario.Instance;
    }

    [Fact] void should_not_be_granted() => _state!.IsGranted.ShouldBeFalse();
}
#endif
