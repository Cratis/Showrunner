// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.for_Offer.when_reducing;

public class an_offer_priced_again : an_offer_read_model
{
    Offer _offer;

    async Task Because()
    {
        await Happened(OfferEvents.Team, OfferEvents.Priced, new OfferPriced("EUR", 9000, 10m));
        _offer = _scenario.Instance;
    }

    [Fact] void should_have_the_latest_currency() => _offer.Currency.ShouldEqual(new CurrencyCode("EUR"));
    [Fact] void should_have_the_latest_price() => _offer.MonthlyPrice.ShouldEqual(new PriceInMinorUnits(9000));
    [Fact] void should_have_the_latest_discount() => _offer.YearlyDiscount.ShouldEqual(new YearlyDiscount(10m));
}
#endif
