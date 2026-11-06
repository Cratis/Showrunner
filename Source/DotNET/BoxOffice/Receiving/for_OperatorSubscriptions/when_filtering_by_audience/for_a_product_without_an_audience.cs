// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Chronicle.EventStoreSubscriptions;

namespace Cratis.Showrunner.BoxOffice.Receiving.for_OperatorSubscriptions.when_filtering_by_audience;

public class for_a_product_without_an_audience : Specification
{
    EventStoreSubscriptionBuilder _subscription;
    EventStoreSubscriptionDefinition _definition;

    void Establish() => _subscription = new(Substitute.For<IEventTypes>(), new EventStoreSubscriptionId("Direct"), "Direct");

    void Because()
    {
        OperatorSubscriptions.FilterByAudience(_subscription, null);
        OperatorSubscriptions.FilterByAudience(_subscription, " ");
        _definition = _subscription.Build();
    }

    [Fact] void should_leave_the_subscription_unfiltered() => _definition.NamedTags.ShouldBeEmpty();
}
#endif
