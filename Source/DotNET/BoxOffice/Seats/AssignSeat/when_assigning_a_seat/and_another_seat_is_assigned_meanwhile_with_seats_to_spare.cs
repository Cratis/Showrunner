// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Seats.AssignSeat.when_assigning_a_seat;

public class and_another_seat_is_assigned_meanwhile_with_seats_to_spare : a_seat_command<AssignSeat>
{
    void Establish()
    {
        Given(new OperatorSetSeatCapacity(5, EntitlementEnd.Never, SeatCapacitySource.Purchase));

        // Another assignment is recorded after this one read the roster and before it appended. Every assignment and
        // release is in the one stream, so the roster this one decided on is out of date even though seats remain: it is
        // refused and retried on what is true now, which is what keeps two assignments from overfilling the pool.
        _scenario.AppendConcurrently(TenantStream.Id, new SeatAssigned(Tenants.Cy));
    }

    async Task Because() => _result = await _scenario.Execute(new AssignSeat(Tenants.Bo));

    [Fact] void should_refuse_the_assignment_that_decided_on_an_old_roster() => _result.ShouldHaveValidationErrorBecauseOf(ValidationResultReason.ConcurrencyViolation);
    [Fact] void should_not_append_it() => _scenario.AppendedEvents.ShouldNotContain(_ => _.Event.Content is SeatAssigned { Holder: var holder } && holder == Tenants.Bo);
}
#endif
