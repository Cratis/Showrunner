// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;
using Cratis.Showrunner.BoxOffice.Catalog.Publishing.for_OfferPublisher.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Publishing.for_OfferPublisher.when_publishing;

public class a_compute_pack : an_offer_publisher
{
    OfferPublished _published;

    async Task Because()
    {
        await Recorded(OfferEvents.ExtraBuilds);
        await Happened(OfferEvents.Priced);
        await ReadOutbox();
        _published = _outbox.Single().Content as OfferPublished;
    }

    [Fact] void should_publish_it_by_name() => _published.Name.ShouldEqual(new OfferName("Extra builds"));
    [Fact] void should_include_no_seats() => _published.IncludedSeats.ShouldEqual(SeatCount.NotSet);
    [Fact] void should_include_no_modules() => _published.Modules.ShouldBeEmpty();
}
#endif
