// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.GuestList.Contracts.Availability;
using AudienceDecision = Cratis.Showrunner.GuestList.Contracts.Availability.AudienceDecision;
using Cratis.Showrunner.GuestList.given;
using Cratis.Showrunner.GuestList.Receiving.for_AudienceReceipts.given;

namespace Cratis.Showrunner.GuestList.Receiving.for_AudienceReceipts.when_receiving;

public class two_decisions_about_the_same_audience : receipts_for_a_product
{
    async Task Because()
    {
        await _receipts.Receive(AudienceDecision.Granted, AudienceLevel.Tenant, Guests.Plus, Recorded(new(Guests.Acme.Value), new("tenant"), Guests.At(5)));
        await _receipts.Receive(AudienceDecision.Withdrawn, AudienceLevel.Tenant, Guests.Plus, Recorded(new(Guests.Acme.Value), new("tenant"), Guests.At(9)));
        await ReadPublished();
    }

    [Fact] void should_publish_a_receipt_for_each() => _published.Count.ShouldEqual(2);
    [Fact] void should_publish_them_in_the_order_they_were_recorded() => Receipt(1).Decision.ShouldEqual(AudienceDecision.Withdrawn);
    [Fact] void should_date_each_by_its_own_decision() => Receipt(1).DecidedAt.ShouldEqual(Guests.At(9));
}
#endif
