// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;
using Cratis.Showrunner.BoxOffice.Catalog.Publishing.for_OfferPublisher.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Publishing.for_OfferPublisher.when_publishing;

public class a_seat_plan_that_gets_its_price : an_offer_publisher
{
    OfferPublished _published;

    async Task Because()
    {
        await Recorded(OfferEvents.Team);
        await Happened(OfferEvents.Priced);
        await ReadOutbox();
        _published = _outbox.Single().Content as OfferPublished;
    }

    [Fact] void should_publish_the_offer() => _published.ShouldNotBeNull();
    [Fact] void should_publish_it_under_the_offer_id() => _outbox.Single().Context.EventSourceId.ShouldEqual((EventSourceId)_offerId);
    [Fact] void should_name_it() => _published.Name.ShouldEqual(new OfferName("Team"));
    [Fact] void should_include_the_seats() => _published.IncludedSeats.ShouldEqual(new SeatCount(10));
    [Fact] void should_include_no_modules() => _published.Modules.ShouldBeEmpty();
    [Fact] void should_publish_the_monthly_and_yearly_price() =>
        _published.Prices.ShouldEqual([new OfferPrice(BillingInterval.Monthly, 10000, "USD"), new OfferPrice(BillingInterval.Yearly, 96000, "USD")]);
}
#endif
