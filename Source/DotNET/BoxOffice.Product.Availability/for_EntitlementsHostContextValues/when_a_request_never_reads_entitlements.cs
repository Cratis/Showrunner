// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Choreography.Availability;
using Cratis.Showrunner.BoxOffice.Product.Availability.given;

namespace Cratis.Showrunner.BoxOffice.Product.Availability.for_EntitlementsHostContextValues;

public class when_a_request_never_reads_entitlements : a_tenant_with_entitlements
{
    async Task Because() => await new AvailabilityEvaluator(new AvailabilityCatalog()).Evaluate(
        AvailabilityExpression.When(new ContextComparison("other", ComparisonOperator.IsTrue)),
        Request());

    [Fact] void should_not_reach_for_the_entitlements_of_the_tenant() => _entitlementsRequested.ShouldEqual(0);
    [Fact] void should_not_ask_what_the_tenant_may_use() => _entitlements.DidNotReceive().SeatCapacity();
}
#endif
