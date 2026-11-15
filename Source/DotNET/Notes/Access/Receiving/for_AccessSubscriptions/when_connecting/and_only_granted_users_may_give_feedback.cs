// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Chronicle.EventStoreSubscriptions;
using Cratis.Chronicle.Reactors;
using Cratis.Showrunner.Notes.Access.Receiving.for_AccessSubscriptions.given;

namespace Cratis.Showrunner.Notes.Access.Receiving.for_AccessSubscriptions.when_connecting;

/// <summary>
/// Only granted users may give feedback, so the application observes the inbox of each access store and subscribes to its
/// outbox, once, however often the worker comes round.
/// </summary>
public class and_only_granted_users_may_give_feedback : access_subscriptions
{
    async Task Because()
    {
        await _connection.Observe(_eventStore);
        await _connection.Observe(_eventStore);
        await _connection.Subscribe(_eventStore);
        await _connection.Subscribe(_eventStore);
    }

    [Fact]
    void should_observe_the_inbox_of_each_access_store_once() =>
        _reactors.Received(2).Register(Arg.Any<ReactorId>(), Arg.Any<Action<IReactorBuilder>>());

    [Fact]
    void should_subscribe_to_each_access_store_once() =>
        _subscriptions.Received(2).Subscribe(Arg.Any<EventStoreSubscriptionId>(), Arg.Any<string>(), Arg.Any<Action<IEventStoreSubscriptionBuilder>>());

    [Fact]
    void should_subscribe_to_the_first_access_store() =>
        _subscriptions.Received(1).Subscribe(AccessSubscriptions.SubscriptionFor("Direct"), "Direct", Arg.Any<Action<IEventStoreSubscriptionBuilder>>());

    [Fact] void should_name_the_subscription_apart_from_the_notes_one() => AccessSubscriptions.SubscriptionFor("Direct").Value.ShouldEqual("notes-access-Direct");
    [Fact] void should_read_the_inbox_of_the_access_store() => AccessSubscriptions.InboxFor("Direct").Value.ShouldEqual("inbox-Direct");
}
#endif
