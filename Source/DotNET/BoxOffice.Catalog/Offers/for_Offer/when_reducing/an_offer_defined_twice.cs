// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.for_Offer.when_reducing;

public class an_offer_defined_twice : an_offer_read_model
{
    Offer _offer;

    async Task Because()
    {
        await Happened(OfferEvents.Team, new OfferDefined(OfferKind.Module, "Something else", "Not the plan."));
        _offer = _scenario.Instance;
    }

    [Fact] void should_keep_the_first_definition() => _offer.Name.ShouldEqual(new OfferName("Team"));
    [Fact] void should_keep_the_kind() => _offer.Kind.ShouldEqual(OfferKind.SeatPlan);
}
#endif
