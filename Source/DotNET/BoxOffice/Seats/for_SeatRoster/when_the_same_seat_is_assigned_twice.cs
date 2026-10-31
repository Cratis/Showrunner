// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Seats.for_SeatRoster;

public class when_the_same_seat_is_assigned_twice : a_tenant_read_model<SeatRoster>
{
    SeatRoster _roster;

    async Task Because()
    {
        await Happened(new SeatAssigned(Tenants.Ada), new SeatAssigned(Tenants.Ada));
        _roster = _scenario.Instance!;
    }

    [Fact] void should_hold_one_seat() => _roster.Held.ShouldEqual(1);
    [Fact] void should_keep_when_it_was_first_assigned() => _roster.Holdings.Single().AssignedAt.ShouldEqual(Tenants.At(0));
}
#endif
