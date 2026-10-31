// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.IncludeAllowanceInOffer.when_including_an_allowance;

public class an_allowance_the_plan_already_includes : an_offer_command<IncludeAllowanceInOffer>
{
    void Establish() => Given(OfferEvents.Team, new OfferIncludedAllowance("build-minutes", 500, AllowancePeriod.Month));

    async Task Because() => _result = await _scenario.Execute(new IncludeAllowanceInOffer(_offerId, "build-minutes", 500, AllowancePeriod.Month));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
