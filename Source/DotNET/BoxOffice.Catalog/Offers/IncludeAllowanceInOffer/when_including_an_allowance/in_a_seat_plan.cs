// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.IncludeAllowanceInOffer.when_including_an_allowance;

public class in_a_seat_plan : an_offer_command<IncludeAllowanceInOffer>
{
    void Establish() => Given(OfferEvents.Team);

    async Task Because() => _result = await _scenario.Execute(new IncludeAllowanceInOffer(_offerId, "build-minutes", 500, AllowancePeriod.Month));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_the_allowance() =>
        await _scenario.ShouldHaveAppendedEvent<IncludeAllowanceInOffer, OfferIncludedAllowance>(_offerId, e => e.Meter == "build-minutes" && e.Quantity == 500 && e.Period == AllowancePeriod.Month);
}
#endif
