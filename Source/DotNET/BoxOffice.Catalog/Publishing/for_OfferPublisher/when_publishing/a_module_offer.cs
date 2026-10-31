// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;
using Cratis.Showrunner.BoxOffice.Catalog.Publishing.for_OfferPublisher.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Publishing.for_OfferPublisher.when_publishing;

public class a_module_offer : an_offer_publisher
{
    OfferPublished _published;

    async Task Because()
    {
        await Recorded(OfferEvents.Reporting);
        await Happened(OfferEvents.Priced);
        await ReadOutbox();
        _published = _outbox.Single().Content as OfferPublished;
    }

    [Fact] void should_include_its_module() => _published.Modules.ShouldEqual([new ModuleId("reporting")]);
    [Fact] void should_include_no_seats() => _published.IncludedSeats.ShouldEqual(SeatCount.NotSet);
}
#endif
