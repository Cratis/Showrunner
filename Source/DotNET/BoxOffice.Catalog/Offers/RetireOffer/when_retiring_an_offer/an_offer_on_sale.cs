// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.RetireOffer.when_retiring_an_offer;

public class an_offer_on_sale : an_offer_command<RetireOffer>
{
    void Establish() => Given(OfferEvents.Team, OfferEvents.Priced);

    async Task Because() => _result = await _scenario.Execute(new RetireOffer(_offerId));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_that_the_offer_was_retired() =>
        await _scenario.ShouldHaveAppendedEvent<RetireOffer, OfferRetired>(_offerId);
}
#endif
