// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Seats.ReleaseSeat.when_releasing_a_seat;

public class and_nobody_holds_the_seat : a_seat_command<ReleaseSeat>
{
    void Establish() => Given(new OperatorSetSeatCapacity(2, EntitlementEnd.Never, SeatCapacitySource.Purchase));

    async Task Because() => _result = await _scenario.Execute(new ReleaseSeat(SeatHolder.Of(Tenants.Bo)));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();
    [Fact] void should_append_nothing() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
