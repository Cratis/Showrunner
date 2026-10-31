// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.IncludeModuleInOffer.when_including_a_module;

public class a_module_the_plan_already_includes : an_offer_command<IncludeModuleInOffer>
{
    void Establish() => Given(OfferEvents.Team, new OfferIncludedModule("reporting"));

    async Task Because() => _result = await _scenario.Execute(new IncludeModuleInOffer(_offerId, "reporting"));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
