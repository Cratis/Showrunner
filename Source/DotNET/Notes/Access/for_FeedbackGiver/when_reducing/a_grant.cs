// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Access.given;

namespace Cratis.Showrunner.Notes.Access.for_FeedbackGiver.when_reducing;

/// <summary>
/// The desk granted the right: the user may give feedback.
/// </summary>
public class a_grant : a_feedback_giver
{
    FeedbackGiver? _state;

    async Task Because()
    {
        await Decided(AccessEvents.Granted());
        _state = _scenario.Instance;
    }

    [Fact] void should_be_granted() => _state!.IsGranted.ShouldBeTrue();
    [Fact] void should_keep_when_the_desk_decided() => _state!.DecidedAt.ShouldEqual(At(0));
    [Fact] void should_be_the_users_right() => _state!.Id.ShouldEqual(_id);
}
#endif
