// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.GuestList.Contracts.Availability;
using AudienceDecision = Cratis.Showrunner.GuestList.Contracts.Availability.AudienceDecision;
using Cratis.Showrunner.GuestList.given;
using Cratis.Showrunner.GuestList.Receiving.for_AudienceReceipts.given;

namespace Cratis.Showrunner.GuestList.Receiving.for_AudienceReceipts.when_receiving;

public class a_decision_delivered_again : receipts_for_a_product
{
    async Task Because()
    {
        var context = Recorded(new(Guests.Acme.Value), new("tenant"), Guests.At(5));
        await _receipts.Receive(AudienceDecision.Granted, AudienceLevel.Tenant, Guests.Plus, context);
        await _receipts.Receive(AudienceDecision.Granted, AudienceLevel.Tenant, Guests.Plus, context);
        await ReadPublished();
    }

    [Fact] void should_publish_one_receipt() => _published.Count.ShouldEqual(1);
}
#endif
