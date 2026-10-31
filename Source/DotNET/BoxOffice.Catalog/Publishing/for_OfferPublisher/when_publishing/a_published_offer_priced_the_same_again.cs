// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;
using Cratis.Showrunner.BoxOffice.Catalog.Publishing.for_OfferPublisher.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Publishing.for_OfferPublisher.when_publishing;

public class a_published_offer_priced_the_same_again : an_offer_publisher
{
    async Task Because()
    {
        await Recorded(OfferEvents.Team, OfferEvents.Priced);
        await Happened(OfferEvents.Priced);
        await ReadOutbox();
    }

    [Fact] void should_publish_nothing() => _outbox.ShouldBeEmpty();
}
#endif
