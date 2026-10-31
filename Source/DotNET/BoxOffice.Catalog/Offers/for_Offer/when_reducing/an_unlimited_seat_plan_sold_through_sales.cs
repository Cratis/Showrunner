// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.for_Offer.when_reducing;

public class an_unlimited_seat_plan_sold_through_sales : an_offer_read_model
{
    Offer _offer;

    async Task Because()
    {
        await Happened(OfferEvents.Enterprise);
        _offer = _scenario.Instance;
    }

    [Fact] void should_have_unlimited_seats() => _offer.UnlimitedSeats.ShouldBeTrue();
    [Fact] void should_require_a_sales_contact() => _offer.RequiresSalesContact.ShouldBeTrue();
}
#endif
