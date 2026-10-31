// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;
using Cratis.Showrunner.BoxOffice.Catalog.Publishing.for_OfferPublisher.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Publishing.for_OfferPublisher.when_publishing;

public class an_unlimited_seat_plan : an_offer_publisher
{
    OfferPublished _published;

    async Task Because()
    {
        await Recorded(OfferEvents.Enterprise);
        await Happened(OfferEvents.Priced);
        await ReadOutbox();
        _published = _outbox.Single().Content as OfferPublished;
    }

    [Fact] void should_publish_the_unlimited_seat_count() => _published.IncludedSeats.ShouldEqual(OfferPublications.UnlimitedSeats);
}
#endif
