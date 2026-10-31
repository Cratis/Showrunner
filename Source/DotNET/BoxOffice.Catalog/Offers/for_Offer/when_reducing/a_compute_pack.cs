// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.for_Offer.when_reducing;

public class a_compute_pack : an_offer_read_model
{
    Offer _offer;

    async Task Because()
    {
        await Happened(OfferEvents.ExtraBuilds);
        _offer = _scenario.Instance;
    }

    [Fact] void should_be_a_compute_pack() => _offer.Kind.ShouldEqual(OfferKind.ComputePack);
    [Fact] void should_extend_the_meter() => _offer.Meter.ShouldEqual(new MeterId("build-minutes"));
    [Fact] void should_add_the_quantity() => _offer.PackQuantity.ShouldEqual(new MeterQuantity(1000));
    [Fact] void should_be_stackable() => _offer.Stackable.ShouldBeTrue();
}
#endif
