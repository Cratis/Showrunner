// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.DefineComputePack.when_defining_a_compute_pack;

public class a_valid_pack : an_offer_command<DefineComputePack>
{
    async Task Because() => _result = await _scenario.Execute(new DefineComputePack(_offerId, "Extra builds", "More minutes.", "build-minutes", 1000, true));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_the_offer_defined() =>
        await _scenario.ShouldHaveAppendedEvent<DefineComputePack, OfferDefined>(_offerId, e => e.Kind == OfferKind.ComputePack);

    [Fact]
    async Task should_append_its_meter() =>
        await _scenario.ShouldHaveAppendedEvent<DefineComputePack, ComputePackSpecified>(_offerId, e => e.Meter == "build-minutes" && e.Quantity == 1000 && e.Stackable);
}
#endif
