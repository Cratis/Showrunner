// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.for_Offer.when_reducing;

public class a_seat_plan : an_offer_read_model
{
    Offer _offer;

    async Task Because()
    {
        await Happened(OfferEvents.Team);
        _offer = _scenario.Instance;
    }

    [Fact] void should_be_the_offer() => _offer.Id.ShouldEqual(_offerId);
    [Fact] void should_be_a_seat_plan() => _offer.Kind.ShouldEqual(OfferKind.SeatPlan);
    [Fact] void should_have_the_name() => _offer.Name.ShouldEqual(new OfferName("Team"));
    [Fact] void should_have_the_seats() => _offer.Seats.ShouldEqual(new SeatCount(10));
    [Fact] void should_not_have_unlimited_seats() => _offer.UnlimitedSeats.ShouldBeFalse();
    [Fact] void should_not_require_a_sales_contact() => _offer.RequiresSalesContact.ShouldBeFalse();
    [Fact] void should_not_be_priced() => _offer.Priced.ShouldBeFalse();
    [Fact] void should_not_be_retired() => _offer.Retired.ShouldBeFalse();
    [Fact] void should_include_nothing() => _offer.IncludedModules.ShouldBeEmpty();
}
#endif
