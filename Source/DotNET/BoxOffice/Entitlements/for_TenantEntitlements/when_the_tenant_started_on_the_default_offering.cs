// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Offering.Starting;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Entitlements.for_TenantEntitlements;

public class when_the_tenant_started_on_the_default_offering : some_entitlements
{
    bool _active;
    bool _hasCore;
    bool _hasAnalytics;
    bool _hasExports;
    int _seats;
    int _seatsAfterTheOperatorDecides;

    async Task Because()
    {
        Given(new TenantStartedOnDefaultOffering("Starter", 3, [new ModuleId("core"), new ModuleId("analytics")]));
        _active = await _entitlements.IsActive();
        _hasCore = await _entitlements.Has(new ModuleId("core"));
        _hasAnalytics = await _entitlements.Has(new ModuleId("analytics"));
        _hasExports = await _entitlements.Has(new ModuleId("exports"));
        _seats = await _entitlements.SeatCapacity();

        Given(new OperatorSetSeatCapacity(8, EntitlementEnd.Never, SeatCapacitySource.Purchase));
        _seatsAfterTheOperatorDecides = await _entitlements.SeatCapacity();
    }

    [Fact] void should_not_be_read_only() => _active.ShouldBeTrue();
    [Fact] void should_admit_the_modules_of_the_offer() => (_hasCore && _hasAnalytics).ShouldBeTrue();
    [Fact] void should_not_admit_a_module_the_offer_leaves_out() => _hasExports.ShouldBeFalse();
    [Fact] void should_have_the_seats_of_the_offer() => _seats.ShouldEqual(3);
    [Fact] void should_have_the_seats_the_operator_decides_later() => _seatsAfterTheOperatorDecides.ShouldEqual(8);
}
#endif
