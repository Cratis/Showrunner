// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Seats.AssignSeat.when_assigning_a_seat;

public class and_someone_else_takes_the_last_seat_meanwhile : a_seat_command<AssignSeat>
{
    void Establish()
    {
        Given(new OperatorSetSeatCapacity(1, EntitlementEnd.Never, SeatCapacitySource.Purchase));

        // Another assignment of the last seat is recorded after this one read the roster and before it appended.
        _scenario.AppendConcurrently(TenantStream.Id, new SeatAssigned(Tenants.Cy));
    }

    async Task Because() => _result = await _scenario.Execute(new AssignSeat(Tenants.Bo));

    [Fact] void should_refuse_the_assignment_that_lost_the_race() => _result.ShouldHaveValidationErrorBecauseOf(ValidationResultReason.ConcurrencyViolation);
    [Fact] void should_not_append_the_assignment_that_lost() => _scenario.AppendedEvents.ShouldNotContain(_ => _.Event.Content is SeatAssigned { Holder: var holder } && holder == Tenants.Bo);
}
#endif
