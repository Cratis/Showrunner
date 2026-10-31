// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Prices.PriceOffer.when_pricing_an_offer;

public class a_defined_offer : an_offer_command<PriceOffer>
{
    void Establish() => Given(OfferEvents.Team);

    async Task Because() => _result = await _scenario.Execute(new PriceOffer(_offerId, "USD", 10000, 20m));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_the_price() =>
        await _scenario.ShouldHaveAppendedEvent<PriceOffer, OfferPriced>(_offerId, e => e.Currency == "USD" && e.MonthlyPrice == 10000 && e.YearlyDiscount == 20m);
}
#endif
