// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.for_Offer.when_reducing;

public class a_retired_offer : an_offer_read_model
{
    Offer _offer;

    async Task Because()
    {
        await Happened(OfferEvents.Team, OfferEvents.Priced, new OfferRetired());
        _offer = _scenario.Instance;
    }

    [Fact] void should_be_retired() => _offer.Retired.ShouldBeTrue();
    [Fact] void should_stay_in_the_catalog_with_its_price() => _offer.MonthlyPrice.ShouldEqual(new PriceInMinorUnits(10000));
}
#endif
