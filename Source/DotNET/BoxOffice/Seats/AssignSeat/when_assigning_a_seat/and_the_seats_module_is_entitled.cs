// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Offering.Declaration;
using Cratis.Showrunner.BoxOffice.Receiving;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.BoxOffice.Seats.AssignSeat.when_assigning_a_seat;

public class and_the_seats_module_is_entitled : a_seat_command<AssignSeat>
{
    void Establish()
    {
        _scenario.Services.AddSingleton(new ProductOfferingBuilder()
            .Product(ProductId.New())
            .Module("seating", "Seating", module => module.Owns("Cratis.Showrunner.BoxOffice.Seats"))
            .Build());
        Given(
            new OperatorGrantedModule("seating", EntitlementEnd.Never),
            new OperatorSetSeatCapacity(2, EntitlementEnd.Never, SeatCapacitySource.Purchase));
    }

    async Task Because() => _result = await _scenario.Execute(new AssignSeat(Tenants.Bo));

    [Fact] void should_let_the_command_through() => _result.ShouldBeSuccessful();
    [Fact] void should_assign_the_seat() => _scenario.AppendedEvents.Select(_ => _.Event.Content).ShouldEqual([new SeatAssigned(Tenants.Bo)]);
}
#endif
