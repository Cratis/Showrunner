// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.Testing.Events;
using Cratis.Showrunner.BoxOffice.Contracts.Offers;
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving.for_OperatorEventTranslator.given;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.BoxOffice.Receiving.for_OperatorEventTranslator.when_translating;

public class and_an_offer_is_published : an_operator_event_translator
{
    readonly OfferId _offerId = OfferId.New();

    protected override IEventStore CreateInbox() => new EventStoreForTesting();

    async Task Because()
    {
        var context = InboxContext(_offerId.ToString());
        await _translator.Translate(new OfferPublished("Team", ["analytics"], 5, [new(BillingInterval.Monthly, 4900, "EUR")]), context, Operator);
        await _translator.Translate(new OfferWithdrawn(), context with { SequenceNumber = 1 }, Operator);
        _recorded = await _inbox.EventLog.GetFromSequenceNumber(EventSequenceNumber.First, _offerId.ToString());
    }

    [Fact] void should_record_the_offer_where_the_operator_published_it() => _recorded.Select(_ => _.Content).OfType<OperatorPublishedOffer>().Single().Name.ShouldEqual(new OfferName("Team"));
    [Fact] void should_record_the_withdrawal() => _recorded.ShouldContain(_ => _.Content is OperatorWithdrewOffer);
    [Fact] void should_not_record_it_in_a_tenant() => _tenantStore.EventLog.GetTailSequenceNumber().Result.ShouldEqual(EventSequenceNumber.Unavailable);
}
#endif
