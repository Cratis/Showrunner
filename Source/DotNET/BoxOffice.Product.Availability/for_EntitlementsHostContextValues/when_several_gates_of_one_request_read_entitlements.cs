// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Choreography.Availability;
using Cratis.Choreography.Availability.Context;
using Cratis.Showrunner.BoxOffice.Product.Availability.given;

namespace Cratis.Showrunner.BoxOffice.Product.Availability.for_EntitlementsHostContextValues;

public class when_several_gates_of_one_request_read_entitlements : a_tenant_with_entitlements
{
    async Task Because()
    {
        var request = Request();
        var evaluator = new AvailabilityEvaluator(new AvailabilityCatalog());
        await evaluator.Evaluate(AvailabilityExpression.When(new ContextComparison("entitlements.modules.analytics.usable", ComparisonOperator.IsTrue)), request);
        await evaluator.Evaluate(AvailabilityExpression.When(new ContextComparison("entitlements.seats", ComparisonOperator.GreaterThan, 1)), request);
        await evaluator.Evaluate(AvailabilityExpression.When(new ContextComparison("entitlements.active", ComparisonOperator.IsTrue)), request);
    }

    [Fact] void should_read_the_entitlements_once() => _entitlementsRequested.ShouldEqual(1);
}
#endif
