// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;
using Cratis.Showrunner.BoxOffice.Catalog.Publishing;

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.for_SeatPlanOffers.when_adopting_a_seat_sku;

public class a_priced_sku : an_offer_read_model
{
    Offer _offer;

    async Task Because()
    {
        var sku = new SeatSku("Team", "For small teams.", 10, false, false);
        await Happened(SeatPlanOffers.From(sku, new SeatSkuPricing("USD", 10000, 20m)).ToArray());
        _offer = _scenario.Instance;
    }

    [Fact] void should_be_a_seat_plan() => _offer.Kind.ShouldEqual(OfferKind.SeatPlan);
    [Fact] void should_keep_the_name() => _offer.Name.ShouldEqual(new OfferName("Team"));
    [Fact] void should_keep_the_description() => _offer.Description.ShouldEqual(new OfferDescription("For small teams."));
    [Fact] void should_keep_the_seats() => _offer.Seats.ShouldEqual(new SeatCount(10));
    [Fact] void should_keep_the_seats_limited() => _offer.UnlimitedSeats.ShouldBeFalse();
    [Fact] void should_keep_the_checkout_route() => _offer.RequiresSalesContact.ShouldBeFalse();
    [Fact] void should_keep_the_price() => _offer.MonthlyPrice.ShouldEqual(new PriceInMinorUnits(10000));
    [Fact] void should_cost_what_the_sku_cost_per_month() => _offer.TotalFor(BillingInterval.Monthly).ShouldEqual(new PriceInMinorUnits(10000));
    [Fact] void should_cost_what_the_sku_cost_per_year() => _offer.TotalFor(BillingInterval.Yearly).ShouldEqual(new PriceInMinorUnits(96000));
    [Fact] void should_be_on_sale() => OfferPublications.For(_offer).ShouldNotBeNull();
    [Fact] void should_publish_the_seats_the_sku_carried() => OfferPublications.For(_offer)!.IncludedSeats.ShouldEqual(new SeatCount(10));
}
#endif
