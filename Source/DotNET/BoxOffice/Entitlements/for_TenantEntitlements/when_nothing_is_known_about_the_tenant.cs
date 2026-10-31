// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Seats;

namespace Cratis.Showrunner.BoxOffice.Entitlements.for_TenantEntitlements;

public class when_nothing_is_known_about_the_tenant : some_entitlements
{
    bool _active;
    bool _hasModule;
    bool _hasSeat;
    int _capacity;
    MeterAllowance _allowance;

    async Task Because()
    {
        _active = await _entitlements.IsActive();
        _hasModule = await _entitlements.Has("analytics");
        _hasSeat = await _entitlements.HasSeat(SeatHolder.Of(Tenants.Ada));
        _capacity = await _entitlements.SeatCapacity();
        _allowance = await _entitlements.AllowanceFor("compute");
    }

    [Fact] void should_not_be_read_only() => _active.ShouldBeTrue();
    [Fact] void should_have_no_modules() => _hasModule.ShouldBeFalse();
    [Fact] void should_have_no_seats() => _hasSeat.ShouldBeFalse();
    [Fact] void should_have_no_capacity() => _capacity.ShouldEqual(0);
    [Fact] void should_allow_nothing_of_a_meter() => _allowance.Quantity.ShouldEqual(MeterQuantity.NotSet);
}
#endif
