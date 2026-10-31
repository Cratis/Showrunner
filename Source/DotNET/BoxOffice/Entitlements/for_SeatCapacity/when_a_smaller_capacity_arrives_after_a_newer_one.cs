// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Entitlements.for_SeatCapacity;

public class when_a_smaller_capacity_arrives_after_a_newer_one : a_tenant_read_model<SeatCapacity>
{
    SeatCapacity _capacity;

    async Task Because()
    {
        await Arrived(
            (Tenants.At(20), new OperatorSetSeatCapacity(8, EntitlementEnd.Never, SeatCapacitySource.Purchase)),
            (Tenants.At(10), new OperatorSetSeatCapacity(2, EntitlementEnd.Never, SeatCapacitySource.Grant)));
        _capacity = _scenario.Instance!;
    }

    [Fact] void should_keep_the_newest_decision() => _capacity.On(Tenants.Today).ShouldEqual(8);
}
#endif
