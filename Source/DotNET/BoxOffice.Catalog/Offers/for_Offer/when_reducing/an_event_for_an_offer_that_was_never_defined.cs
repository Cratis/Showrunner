// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.for_Offer.when_reducing;

public class an_event_for_an_offer_that_was_never_defined : an_offer_read_model
{
    Offer? _offer;

    async Task Because()
    {
        await Happened(OfferEvents.Priced, new OfferIncludedModule("reporting"), new OfferRetired());
        _offer = _scenario.Instance;
    }

    [Fact] void should_not_list_an_offer() => _offer.ShouldBeNull();
}
#endif
