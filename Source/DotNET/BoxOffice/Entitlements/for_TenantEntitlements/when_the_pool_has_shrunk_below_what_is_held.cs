// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Entitlements.for_TenantEntitlements;

public class when_the_pool_has_shrunk_below_what_is_held : some_entitlements
{
    bool _ada;
    bool _bo;
    bool _cy;

    async Task Because()
    {
        Given(
            new OperatorSetSeatCapacity(3, EntitlementEnd.Never, SeatCapacitySource.Purchase),
            new Seats.SeatAssigned(Tenants.Ada),
            new Seats.SeatAssigned(Tenants.Bo),
            new Seats.SeatAssigned(Tenants.Cy),
            new OperatorSetSeatCapacity(2, EntitlementEnd.Never, SeatCapacitySource.Purchase));
        _ada = await _entitlements.HasSeat(Seats.SeatHolder.Of(Tenants.Ada));
        _bo = await _entitlements.HasSeat(Seats.SeatHolder.Of(Tenants.Bo));
        _cy = await _entitlements.HasSeat(Seats.SeatHolder.Of(Tenants.Cy));
    }

    [Fact] void should_license_the_first_holder() => _ada.ShouldBeTrue();
    [Fact] void should_license_the_second_holder() => _bo.ShouldBeTrue();
    [Fact] void should_not_license_the_last_holder() => _cy.ShouldBeFalse();
}
#endif
