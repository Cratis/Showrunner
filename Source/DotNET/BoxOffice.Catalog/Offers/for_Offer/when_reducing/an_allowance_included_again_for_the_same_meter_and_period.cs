// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.for_Offer.when_reducing;

public class an_allowance_included_again_for_the_same_meter_and_period : an_offer_read_model
{
    Offer _offer;

    async Task Because()
    {
        await Happened(OfferEvents.Team, new OfferIncludedAllowance("build-minutes", 100, AllowancePeriod.Month), new OfferIncludedAllowance("build-minutes", 100, AllowancePeriod.Day), new OfferIncludedAllowance("build-minutes", 500, AllowancePeriod.Month));
        _offer = _scenario.Instance;
    }

    [Fact] void should_keep_one_allowance_per_meter_and_period() => _offer.IncludedAllowances.Count().ShouldEqual(2);
    [Fact] void should_replace_the_earlier_allowance() => _offer.IncludedAllowances.ShouldContain(new OfferAllowance("build-minutes", 500, AllowancePeriod.Month));
    [Fact] void should_keep_the_allowance_of_the_other_period() => _offer.IncludedAllowances.ShouldContain(new OfferAllowance("build-minutes", 100, AllowancePeriod.Day));
}
#endif
