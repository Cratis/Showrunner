// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.IncludeModuleInOffer.when_including_a_module;

public class in_a_seat_plan : an_offer_command<IncludeModuleInOffer>
{
    void Establish() => Given(OfferEvents.Team);

    async Task Because() => _result = await _scenario.Execute(new IncludeModuleInOffer(_offerId, "reporting"));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_the_module() =>
        await _scenario.ShouldHaveAppendedEvent<IncludeModuleInOffer, OfferIncludedModule>(_offerId, e => e.Module == "reporting");
}
#endif
