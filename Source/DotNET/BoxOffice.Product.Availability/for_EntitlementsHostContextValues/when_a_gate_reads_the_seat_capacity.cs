// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Choreography.Availability;
using Cratis.Showrunner.BoxOffice.Product.Availability.given;

namespace Cratis.Showrunner.BoxOffice.Product.Availability.for_EntitlementsHostContextValues;

public class when_a_gate_reads_the_seat_capacity : a_tenant_with_entitlements
{
    AvailabilityResult _enough;
    AvailabilityResult _tooFew;

    async Task Because()
    {
        _enough = await Evaluate("entitlements.seats", ComparisonOperator.GreaterThanOrEqual, 5);
        _tooFew = await Evaluate("entitlements.seats", ComparisonOperator.GreaterThan, 5);
    }

    [Fact] void should_hold_when_the_capacity_covers_it() => _enough.IsAvailable.ShouldBeTrue();
    [Fact] void should_not_hold_when_it_does_not() => _tooFew.Outcome.ShouldEqual(AvailabilityOutcome.Unavailable);
}
#endif
