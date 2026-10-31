// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Gating;
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Offering.Declaration;
using Cratis.Showrunner.BoxOffice.Receiving;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.BoxOffice.Seats.AssignSeat.when_assigning_a_seat;

public class and_the_seats_module_is_not_entitled : a_seat_command<AssignSeat>
{
    void Establish()
    {
        // A product that sells seat management as a module owns the namespace the command lives in.
        _scenario.Services.AddSingleton(new ProductOfferingBuilder()
            .Product(ProductId.New())
            .Module("seating", "Seating", module => module.Owns("Cratis.Showrunner.BoxOffice.Seats"))
            .Build());
        Given(new OperatorSetSeatCapacity(2, EntitlementEnd.Never, SeatCapacitySource.Purchase));
    }

    async Task Because() => _result = await _scenario.Execute(new AssignSeat(Tenants.Bo));

    [Fact] void should_refuse_the_command_through_the_pipeline() => _result.ShouldNotBeAuthorized();
    [Fact] void should_say_the_module_is_not_entitled() => _result.AuthorizationFailureReason.ShouldEqual(GatingReasons.ModuleNotEntitled);
    [Fact] void should_append_nothing() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
