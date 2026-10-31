// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;
using Cratis.Showrunner.BoxOffice.Catalog.Publishing.for_OfferPublisher.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Publishing.for_OfferPublisher.when_publishing;

public class a_published_offer_that_gets_an_allowance : an_offer_publisher
{
    async Task Because()
    {
        await Recorded(OfferEvents.Team, OfferEvents.Priced);
        await Happened(new OfferIncludedAllowance("build-minutes", 500, AllowancePeriod.Month));
        await ReadOutbox();
    }

    [Fact] void should_publish_nothing_because_the_contract_does_not_carry_allowances() => _outbox.ShouldBeEmpty();
}
#endif
