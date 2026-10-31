// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Seats.AssignSeat.when_assigning_a_seat;

public class and_the_tenant_is_suspended : a_seat_command<AssignSeat>
{
    void Establish() => Given(
        new OperatorSetSeatCapacity(2, EntitlementEnd.Never, SeatCapacitySource.Purchase),
        new OperatorSuspendedTenant(SuspensionCause.PaymentOverdue));

    async Task Because() => _result = await _scenario.Execute(new AssignSeat(Tenants.Bo));

    [Fact] void should_refuse_the_command_through_the_pipeline() => _result.ShouldNotBeAuthorized();
    [Fact] void should_say_the_tenant_is_suspended() => _result.AuthorizationFailureReason.ShouldEqual("TenantSuspended");
    [Fact] void should_append_nothing() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
