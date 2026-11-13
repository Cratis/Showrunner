// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.GuestList.Contracts.Availability;
using AudienceDecision = Cratis.Showrunner.GuestList.Contracts.Availability.AudienceDecision;
using Cratis.Showrunner.GuestList.given;
using Cratis.Showrunner.GuestList.Receiving.for_AudienceReceipts.given;

namespace Cratis.Showrunner.GuestList.Receiving.for_AudienceReceipts.when_receiving;

public class a_decision_recorded_for_a_user_and_for_the_system : receipts_for_a_product
{
    async Task Because()
    {
        await _receipts.Receive(AudienceDecision.Withdrawn, AudienceLevel.User, Guests.Preview, Recorded(new(Guests.Acme.Value), new($"user:{Guests.Alice.Value}"), Guests.At(5)));
        await _receipts.Receive(AudienceDecision.Granted, AudienceLevel.System, Guests.Internal, Recorded(EventStoreNamespaceName.Default, new("system"), Guests.At(6)));
        await ReadPublished();
    }

    [Fact] void should_name_the_tenant_of_the_user() => Receipt(0).TenantId.ShouldEqual(Guests.Acme);
    [Fact] void should_name_the_user() => Receipt(0).UserId.ShouldEqual(Guests.Alice);
    [Fact] void should_say_the_user_was_withdrawn() => Receipt(0).Decision.ShouldEqual(AudienceDecision.Withdrawn);
    [Fact] void should_name_no_tenant_for_the_system() => Receipt(1).TenantId.ShouldEqual(TenantId.NotSet);
    [Fact] void should_name_no_user_for_the_system() => Receipt(1).UserId.ShouldEqual(UserId.NotSet);
    [Fact] void should_say_the_system_level() => Receipt(1).Level.ShouldEqual(AudienceLevel.System);
}
#endif
