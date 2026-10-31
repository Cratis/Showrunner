// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Seats.AssignSeat.when_assigning_a_seat;

public class and_the_pool_has_shrunk_below_what_is_held : a_seat_command<AssignSeat>
{
    void Establish() => Given(
        new OperatorSetSeatCapacity(2, EntitlementEnd.Never, SeatCapacitySource.Purchase),
        new SeatAssigned(Tenants.Ada),
        new SeatAssigned(Tenants.Bo),
        new OperatorSetSeatCapacity(1, EntitlementEnd.Never, SeatCapacitySource.Purchase));

    async Task Because() => _result = await _scenario.Execute(new AssignSeat(Tenants.Cy));

    [Fact] void should_have_validation_errors() => _result.ShouldHaveValidationErrors();
    [Fact] void should_append_nothing() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
