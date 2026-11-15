// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.Access.for_AccessSubscriptions.given;

namespace Cratis.Showrunner.Notes.Collecting.Access.for_AccessSubscriptions.when_connecting;

/// <summary>
/// No access store is named, the default: a host whose applications share its kernel needs no desk, so the collecting store
/// observes and subscribes to nothing.
/// </summary>
public class and_no_access_store_is_named : access_subscriptions
{
    protected override IEnumerable<string> Stores => [];

    async Task Because()
    {
        await _connection.Observe(_eventStore);
        await _connection.Subscribe(_eventStore);
    }

    [Fact] void should_not_observe_anything() => _reactors.ReceivedCalls().ShouldBeEmpty();
    [Fact] void should_not_subscribe_to_anything() => _subscriptions.ReceivedCalls().ShouldBeEmpty();
}
#endif
