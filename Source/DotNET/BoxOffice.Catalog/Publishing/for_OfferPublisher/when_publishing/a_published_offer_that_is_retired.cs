// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;
using Cratis.Showrunner.BoxOffice.Catalog.Publishing.for_OfferPublisher.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Publishing.for_OfferPublisher.when_publishing;

public class a_published_offer_that_is_retired : an_offer_publisher
{
    async Task Because()
    {
        await Recorded(OfferEvents.Team, OfferEvents.Priced);
        await Happened(new OfferRetired());
        await ReadOutbox();
    }

    [Fact] void should_publish_that_it_was_withdrawn() => _outbox.Single().Content.ShouldBeOfExactType<OfferWithdrawn>();
    [Fact] void should_publish_it_under_the_offer_id() => _outbox.Single().Context.EventSourceId.ShouldEqual((EventSourceId)_offerId);
}
#endif
