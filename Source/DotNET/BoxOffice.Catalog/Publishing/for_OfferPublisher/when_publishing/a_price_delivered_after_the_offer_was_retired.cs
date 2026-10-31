// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.BoxOffice.Catalog.given;
using Cratis.Showrunner.BoxOffice.Catalog.Publishing.for_OfferPublisher.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Publishing.for_OfferPublisher.when_publishing;

public class a_price_delivered_after_the_offer_was_retired : an_offer_publisher
{
    async Task Because()
    {
        await Recorded(OfferEvents.Team, OfferEvents.Priced, new OfferRetired());
        var first = (await _eventStore.GetEventSequence(EventSequenceId.Log).GetFromSequenceNumber(EventSequenceNumber.First)).Single(appended => appended.Content is OfferPriced);
        await Deliver(first);
        await ReadOutbox();
    }

    [Fact] void should_decide_from_the_events_at_the_time_of_the_delivery() => _outbox.Single().Content.ShouldBeOfExactType<OfferPublished>();
}
#endif
