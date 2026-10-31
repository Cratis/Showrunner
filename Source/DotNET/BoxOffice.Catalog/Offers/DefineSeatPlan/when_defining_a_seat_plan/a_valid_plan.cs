// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.DefineSeatPlan.when_defining_a_seat_plan;

public class a_valid_plan : an_offer_command<DefineSeatPlan>
{
    async Task Because() => _result = await _scenario.Execute(new DefineSeatPlan(_offerId, "Team", "For small teams.", 10, false, false));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_the_offer_defined() =>
        await _scenario.ShouldHaveAppendedEvent<DefineSeatPlan, OfferDefined>(_offerId, e => e.Kind == OfferKind.SeatPlan && e.Name == "Team");

    [Fact]
    async Task should_append_its_seats() =>
        await _scenario.ShouldHaveAppendedEvent<DefineSeatPlan, SeatPlanSpecified>(_offerId, e => e.Seats == 10 && !e.Unlimited);
}
#endif
