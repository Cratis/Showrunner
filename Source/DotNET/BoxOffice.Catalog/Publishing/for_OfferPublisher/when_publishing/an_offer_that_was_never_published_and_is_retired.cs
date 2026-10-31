// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;
using Cratis.Showrunner.BoxOffice.Catalog.Publishing.for_OfferPublisher.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Publishing.for_OfferPublisher.when_publishing;

public class an_offer_that_was_never_published_and_is_retired : an_offer_publisher
{
    async Task Because()
    {
        await Recorded(OfferEvents.Team);
        await Happened(new OfferRetired());
        await ReadOutbox();
    }

    [Fact] void should_publish_nothing() => _outbox.ShouldBeEmpty();
}
#endif
