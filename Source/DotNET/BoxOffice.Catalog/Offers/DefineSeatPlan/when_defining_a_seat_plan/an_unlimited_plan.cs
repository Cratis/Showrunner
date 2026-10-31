// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.DefineSeatPlan.when_defining_a_seat_plan;

public class an_unlimited_plan : an_offer_command<DefineSeatPlan>
{
    async Task Because() => _result = await _scenario.Execute(new DefineSeatPlan(_offerId, "Enterprise", "For everyone.", 0, true, true));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_it_as_unlimited_and_sold_through_sales() =>
        await _scenario.ShouldHaveAppendedEvent<DefineSeatPlan, SeatPlanSpecified>(_offerId, e => e.Unlimited && e.RequiresSalesContact);
}
#endif
