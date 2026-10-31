// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.BoxOffice.Catalog.given;
using Cratis.Showrunner.BoxOffice.Catalog.Publishing.for_OfferPublisher.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Publishing.for_OfferPublisher.when_publishing;

public class the_same_delivery_twice : an_offer_publisher
{
    async Task Because()
    {
        await Recorded(OfferEvents.Team);
        await Happened(OfferEvents.Priced);
        var log = await _eventStore.GetEventSequence(EventSequenceId.Log).GetFromSequenceNumber(EventSequenceNumber.First);
        var priced = log[^1];
        await Deliver(priced);
        await ReadOutbox();
    }

    [Fact] void should_publish_it_once() => _outbox.Count.ShouldEqual(1);
}
#endif
