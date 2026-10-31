// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Execution;
using Cratis.Showrunner.BoxOffice.Contracts.Offers;
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Offers.for_Offer;

public class when_an_offer_is_published_and_withdrawn : Specification
{
    readonly OfferId _offerId = OfferId.New();
    ReadModelScenario<Offer> _scenario;
    Offer _published;
    Offer _withdrawn;

    void Establish() => _scenario = new();

    void Destroy() => _scenario.Dispose();

    async Task Because()
    {
        await Process(0, new OperatorPublishedOffer("Team", ["analytics"], 5, [new(BillingInterval.Monthly, 4900, "EUR")]));
        _published = _scenario.Instance!;
        await Process(1, new OperatorWithdrewOffer());
        _withdrawn = _scenario.Instance!;
    }

    [Fact] void should_be_on_sale_once_published() => _published.Withdrawn.ShouldBeFalse();
    [Fact] void should_carry_the_offers_name() => _published.Name.ShouldEqual(new OfferName("Team"));
    [Fact] void should_carry_the_included_seats() => _published.IncludedSeats.ShouldEqual(new SeatCount(5));
    [Fact] void should_be_named_by_the_offer_id() => _published.Id.ShouldEqual(_offerId);
    [Fact] void should_be_withdrawn_once_withdrawn() => _withdrawn.Withdrawn.ShouldBeTrue();
    [Fact] void should_keep_what_the_offer_was() => _withdrawn.Name.ShouldEqual(new OfferName("Team"));

    Task Process(ulong sequenceNumber, object @event) =>
        _scenario.ProcessAppendedEvents([new AppendedEvent(
            EventContext.From(
                EventStoreName.NotSet,
                EventStoreNamespaceName.NotSet,
                EventType.Unknown,
                EventSourceType.Default,
                _offerId,
                EventStreamType.All,
                EventStreamId.Default,
                sequenceNumber,
                CorrelationId.New(),
                Tenants.At((int)sequenceNumber)),
            @event)]);
}
#endif
