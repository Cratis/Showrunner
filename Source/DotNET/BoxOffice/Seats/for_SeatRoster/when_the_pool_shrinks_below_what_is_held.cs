// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Seats.for_SeatRoster;

public class when_the_pool_shrinks_below_what_is_held : a_tenant_read_model<SeatRoster>
{
    SeatRoster _roster;

    async Task Because()
    {
        await Happened(new SeatAssigned(Tenants.Ada), new SeatAssigned(Tenants.Bo), new SeatAssigned(Tenants.Cy));
        _roster = _scenario.Instance!;
    }

    [Fact] void should_license_the_oldest_up_to_the_capacity() => _roster.Licensed(2).Select(_ => _.Holder).ShouldEqual([Tenants.Ada, Tenants.Bo]);
    [Fact] void should_license_the_first_holder() => _roster.IsLicensed(SeatHolder.Of(Tenants.Ada), 2).ShouldBeTrue();
    [Fact] void should_not_license_the_newest_holder() => _roster.IsLicensed(SeatHolder.Of(Tenants.Cy), 2).ShouldBeFalse();
    [Fact] void should_still_hold_the_unlicensed_seat() => _roster.Holds(SeatHolder.Of(Tenants.Cy)).ShouldBeTrue();
    [Fact] void should_license_nobody_without_capacity() => _roster.Licensed(0).ShouldBeEmpty();
}
#endif
