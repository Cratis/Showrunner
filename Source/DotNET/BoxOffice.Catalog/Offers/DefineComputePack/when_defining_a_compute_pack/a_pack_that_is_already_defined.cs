// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.DefineComputePack.when_defining_a_compute_pack;

public class a_pack_that_is_already_defined : an_offer_command<DefineComputePack>
{
    void Establish() => Given(OfferEvents.ExtraBuilds);

    async Task Because() => _result = await _scenario.Execute(new DefineComputePack(_offerId, "Extra builds", "More minutes.", "build-minutes", 1000, true));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();
    [Fact] void should_have_validation_errors() => _result.ShouldHaveValidationErrors();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
