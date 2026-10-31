// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Seats.AssignSeat.when_assigning_a_seat;

public class and_a_seat_is_free : a_seat_command<AssignSeat>
{
    void Establish() => Given(new OperatorSetSeatCapacity(2, EntitlementEnd.Never, SeatCapacitySource.Purchase));

    async Task Because() => _result = await _scenario.Execute(new AssignSeat(Tenants.Bo));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();
    [Fact] void should_append_the_assignment() => _scenario.AppendedEvents.Select(_ => _.Event.Content).ShouldEqual([new SeatAssigned(Tenants.Bo)]);
    [Fact] async Task should_append_it_to_the_tenants_stream() => await _scenario.ShouldHaveAppendedEvent<AssignSeat, SeatAssigned>(TenantStream.Id, e => e.Holder == Tenants.Bo);
}
#endif
