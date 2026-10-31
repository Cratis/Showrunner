// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.DefineModuleOffer.when_defining_a_module_offer;

public class an_offer_without_a_name : an_offer_command<DefineModuleOffer>
{
    async Task Because() => _result = await _scenario.Execute(new DefineModuleOffer(_offerId, "", "Reports.", "reporting", ModuleCharging.PerSeat, 14, ModuleAvailability.Paid));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();
    [Fact] void should_have_validation_errors() => _result.ShouldHaveValidationErrors();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
