// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.for_Offer.when_reducing;

public class a_priced_offer : an_offer_read_model
{
    Offer _offer;

    async Task Because()
    {
        await Happened(OfferEvents.Team, OfferEvents.Priced);
        _offer = _scenario.Instance;
    }

    [Fact] void should_be_priced() => _offer.Priced.ShouldBeTrue();
    [Fact] void should_have_the_currency() => _offer.Currency.ShouldEqual(new CurrencyCode("USD"));
    [Fact] void should_have_the_monthly_price() => _offer.MonthlyPrice.ShouldEqual(new PriceInMinorUnits(10000));
    [Fact] void should_cost_the_monthly_price_per_month() => _offer.TotalFor(BillingInterval.Monthly).ShouldEqual(new PriceInMinorUnits(10000));
    [Fact] void should_cost_twelve_months_less_the_discount_per_year() => _offer.TotalFor(BillingInterval.Yearly).ShouldEqual(new PriceInMinorUnits(96000));
}
#endif
