// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Access.given;

namespace Cratis.Showrunner.Notes.Access.for_FeedbackGiver.when_reducing;

/// <summary>
/// The desk granted and then revoked, but the revocation reaches the application first: the later decision wins, so the user may not give feedback.
/// </summary>
public class a_revocation_that_arrives_before_the_grant : a_feedback_giver
{
    FeedbackGiver? _state;

    async Task Because()
    {
        await DecidedAt((AccessEvents.Revoked(), At(5)), (AccessEvents.Granted(), At(4)));
        _state = _scenario.Instance;
    }

    [Fact] void should_not_be_granted() => _state!.IsGranted.ShouldBeFalse();
    [Fact] void should_keep_when_the_desk_revoked() => _state!.DecidedAt.ShouldEqual(At(5));
}
#endif
