// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.for_Offer.when_reducing;

public class a_module_included_twice : an_offer_read_model
{
    Offer _offer;

    async Task Because()
    {
        await Happened(OfferEvents.Team, new OfferIncludedModule("reporting"), new OfferIncludedModule("audit"), new OfferIncludedModule("reporting"));
        _offer = _scenario.Instance;
    }

    [Fact] void should_include_each_module_once() => _offer.IncludedModules.ShouldEqual([new ModuleId("reporting"), new ModuleId("audit")]);
}
#endif
