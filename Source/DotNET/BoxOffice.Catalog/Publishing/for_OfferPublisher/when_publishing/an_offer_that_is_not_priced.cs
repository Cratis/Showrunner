// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;
using Cratis.Showrunner.BoxOffice.Catalog.Publishing.for_OfferPublisher.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Publishing.for_OfferPublisher.when_publishing;

public class an_offer_that_is_not_priced : an_offer_publisher
{
    async Task Because()
    {
        await Happened(OfferEvents.Team[0]);
        await Happened(OfferEvents.Team[1]);
        await Happened(new OfferIncludedModule("reporting"));
        await ReadOutbox();
    }

    [Fact] void should_publish_nothing() => _outbox.ShouldBeEmpty();
}
#endif
