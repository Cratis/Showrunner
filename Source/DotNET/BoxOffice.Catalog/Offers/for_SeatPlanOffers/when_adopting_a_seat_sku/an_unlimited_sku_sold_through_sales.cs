// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;
using Cratis.Showrunner.BoxOffice.Catalog.Publishing;

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.for_SeatPlanOffers.when_adopting_a_seat_sku;

public class an_unlimited_sku_sold_through_sales : an_offer_read_model
{
    Offer _offer;

    async Task Because()
    {
        var sku = new SeatSku("Enterprise", "For everyone.", 0, true, true);
        await Happened(SeatPlanOffers.From(sku, new SeatSkuPricing("EUR", 250000, 10m)).ToArray());
        _offer = _scenario.Instance;
    }

    [Fact] void should_keep_the_seats_unlimited() => _offer.UnlimitedSeats.ShouldBeTrue();
    [Fact] void should_keep_the_sales_route() => _offer.RequiresSalesContact.ShouldBeTrue();
    [Fact] void should_keep_the_currency() => _offer.Currency.ShouldEqual(new CurrencyCode("EUR"));
    [Fact] void should_publish_unlimited_seats() => OfferPublications.For(_offer)!.IncludedSeats.ShouldEqual(OfferPublications.UnlimitedSeats);
}
#endif
