// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.DefineModuleOffer.when_defining_a_module_offer;

public class a_valid_module_offer : an_offer_command<DefineModuleOffer>
{
    async Task Because() => _result = await _scenario.Execute(new DefineModuleOffer(_offerId, "Reporting", "Reports.", "reporting", ModuleCharging.PerSeat, 14, ModuleAvailability.Paid));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_the_offer_defined() =>
        await _scenario.ShouldHaveAppendedEvent<DefineModuleOffer, OfferDefined>(_offerId, e => e.Kind == OfferKind.Module);

    [Fact]
    async Task should_append_its_module() =>
        await _scenario.ShouldHaveAppendedEvent<DefineModuleOffer, ModuleOfferSpecified>(_offerId, e => e.Module == "reporting" && e.Charging == ModuleCharging.PerSeat && e.Trial == 14);
}
#endif
