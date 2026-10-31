// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;
using Cratis.Showrunner.BoxOffice.Catalog.Publishing;

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.for_SeatPlanOffers.when_adopting_a_seat_sku;

public class a_sku_that_is_not_priced : an_offer_read_model
{
    Offer _offer;

    async Task Because()
    {
        var sku = new SeatSku("Free", "Try it.", 2, false, false);
        await Happened(SeatPlanOffers.From(sku).ToArray());
        _offer = _scenario.Instance;
    }

    [Fact] void should_be_a_seat_plan() => _offer.Kind.ShouldEqual(OfferKind.SeatPlan);
    [Fact] void should_not_be_priced() => _offer.Priced.ShouldBeFalse();
    [Fact] void should_not_be_on_sale() => OfferPublications.For(_offer).ShouldBeNull();
}
#endif
