// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.IncludeAllowanceInOffer.when_including_an_allowance;

public class no_meter : an_offer_command<IncludeAllowanceInOffer>
{
    void Establish() => Given(OfferEvents.Team);

    async Task Because() => _result = await _scenario.Execute(new IncludeAllowanceInOffer(_offerId, "", 500, AllowancePeriod.Month));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();
    [Fact] void should_have_validation_errors() => _result.ShouldHaveValidationErrors();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
