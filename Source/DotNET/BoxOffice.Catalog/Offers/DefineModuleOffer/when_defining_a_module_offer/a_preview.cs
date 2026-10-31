// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.DefineModuleOffer.when_defining_a_module_offer;

public class a_preview : an_offer_command<DefineModuleOffer>
{
    async Task Because() => _result = await _scenario.Execute(new DefineModuleOffer(_offerId, "Reporting", "Reports.", "reporting", ModuleCharging.PerOrganization, 0, ModuleAvailability.Preview));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_it_as_a_preview() =>
        await _scenario.ShouldHaveAppendedEvent<DefineModuleOffer, ModuleOfferSpecified>(_offerId, e => e.Availability == ModuleAvailability.Preview);
}
#endif
