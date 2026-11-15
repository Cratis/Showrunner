// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Access.Receiving.for_AccessSubscriptions.given;

namespace Cratis.Showrunner.Notes.Access.Receiving.for_AccessSubscriptions.when_connecting;

/// <summary>
/// Everyone may give feedback, so the application does not touch the desk: the default configured desk is not subscribed to
/// by an application that never asked for access control.
/// </summary>
public class and_everyone_may_give_feedback : access_subscriptions
{
    protected override FeedbackAccessMode AccessMode => FeedbackAccessMode.Everyone;

    async Task Because()
    {
        await _connection.Observe(_eventStore);
        await _connection.Subscribe(_eventStore);
    }

    [Fact] void should_not_observe_anything() => _reactors.ReceivedCalls().ShouldBeEmpty();
    [Fact] void should_not_subscribe_to_anything() => _subscriptions.ReceivedCalls().ShouldBeEmpty();
}
#endif
