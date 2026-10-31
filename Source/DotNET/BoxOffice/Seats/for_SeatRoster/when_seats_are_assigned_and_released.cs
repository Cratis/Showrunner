// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Seats.for_SeatRoster;

public class when_seats_are_assigned_and_released : a_tenant_read_model<SeatRoster>
{
    SeatRoster _roster;

    async Task Because()
    {
        await Happened(new SeatAssigned(Tenants.Ada), new SeatAssigned(Tenants.Bo), new SeatUnassigned(Tenants.Ada));
        _roster = _scenario.Instance!;
    }

    [Fact] void should_hold_one_seat() => _roster.Held.ShouldEqual(1);
    [Fact] void should_hold_it_for_the_one_who_kept_it() => _roster.Holds(SeatHolder.Of(Tenants.Bo)).ShouldBeTrue();
    [Fact] void should_not_hold_it_for_the_one_who_released_it() => _roster.Holds(SeatHolder.Of(Tenants.Ada)).ShouldBeFalse();
}
#endif
