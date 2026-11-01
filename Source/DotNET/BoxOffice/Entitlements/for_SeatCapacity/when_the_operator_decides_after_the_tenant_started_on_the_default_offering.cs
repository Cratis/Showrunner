// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Offering.Starting;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Entitlements.for_SeatCapacity;

public class when_the_operator_decides_after_the_tenant_started_on_the_default_offering : a_tenant_read_model<SeatCapacity>
{
    SeatCapacity _capacity;

    async Task Because()
    {
        await Happened(
            new TenantStartedOnDefaultOffering("Starter", 3, [new ModuleId("core")]),
            new OperatorSetSeatCapacity(10, EntitlementEnd.Never, SeatCapacitySource.Purchase));
        _capacity = _scenario.Instance!;
    }

    [Fact] void should_have_the_seats_the_operator_decided() => _capacity.On(Tenants.Today).ShouldEqual(10);
}
#endif
