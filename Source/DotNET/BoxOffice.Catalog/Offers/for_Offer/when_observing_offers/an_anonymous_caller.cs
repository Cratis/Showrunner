// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Showrunner.BoxOffice.Catalog.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.for_Offer.when_observing_offers;

public class an_anonymous_caller : a_catalog_query
{
    protected override ClaimsPrincipal Principal => new(new ClaimsIdentity());

    async Task Because() => _result = await _scenario.Perform(nameof(Offer.OffersOnSale));

    [Fact] void should_not_be_authorized() => _result.IsAuthorized.ShouldBeFalse();
}
#endif
