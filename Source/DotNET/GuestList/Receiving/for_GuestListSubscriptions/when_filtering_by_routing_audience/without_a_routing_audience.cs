// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Chronicle.EventStoreSubscriptions;

namespace Cratis.Showrunner.GuestList.Receiving.for_GuestListSubscriptions.when_filtering_by_routing_audience;

/// <summary>
/// Without a routing audience the subscription is not narrowed.
/// </summary>
public class without_a_routing_audience : Specification
{
    EventStoreSubscriptionBuilder _subscription;
    EventStoreSubscriptionDefinition _definition;

    void Establish() => _subscription = new(Substitute.For<IEventTypes>(), new EventStoreSubscriptionId("Direct"), "Direct");

    void Because()
    {
        GuestListSubscriptions.FilterByRoutingAudience(_subscription, null);
        GuestListSubscriptions.FilterByRoutingAudience(_subscription, " ");
        _definition = _subscription.Build();
    }

    [Fact] void should_not_narrow_the_subscription() => _definition.NamedTags.ShouldBeEmpty();
}
#endif
