// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Prices.PriceOffer.when_pricing_an_offer;

public class a_discount_above_a_hundred_percent : an_offer_command<PriceOffer>
{
    void Establish() => Given(OfferEvents.Team);

    async Task Because() => _result = await _scenario.Execute(new PriceOffer(_offerId, "USD", 10000, 100.5m));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();
    [Fact] void should_have_validation_errors() => _result.ShouldHaveValidationErrors();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
