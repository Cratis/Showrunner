// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Chronicle.EventStoreSubscriptions;

namespace Cratis.Showrunner.GuestList.Receiving.for_GuestListSubscriptions.when_filtering_by_routing_audience;

/// <summary>
/// The subscription is narrowed with Chronicle's routing audience, which is a label of the subscription and not a field of
/// any event.
/// </summary>
public class with_a_routing_audience : Specification
{
    EventStoreSubscriptionBuilder _subscription;
    EventStoreSubscriptionDefinition _definition;

    void Establish() => _subscription = new(Substitute.For<IEventTypes>(), new EventStoreSubscriptionId("Direct"), "Direct");

    void Because()
    {
        GuestListSubscriptions.FilterByRoutingAudience(_subscription, "ledger");
        _definition = _subscription.Build();
    }

    [Fact] void should_subscribe_for_the_routing_audience() => _definition.NamedTags.ShouldContainOnly(new NamedTag(WellKnownTags.Audience, "ledger"));
}
#endif
