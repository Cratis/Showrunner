// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.IncludeModuleInOffer.when_including_a_module;

public class no_module : an_offer_command<IncludeModuleInOffer>
{
    void Establish() => Given(OfferEvents.Team);

    async Task Because() => _result = await _scenario.Execute(new IncludeModuleInOffer(_offerId, ""));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();
    [Fact] void should_have_validation_errors() => _result.ShouldHaveValidationErrors();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
