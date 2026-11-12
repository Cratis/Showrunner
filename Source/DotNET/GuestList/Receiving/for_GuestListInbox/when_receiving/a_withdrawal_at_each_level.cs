// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.given;
using Cratis.Showrunner.GuestList.Receiving.for_GuestListInbox.given;

namespace Cratis.Showrunner.GuestList.Receiving.for_GuestListInbox.when_receiving;

/// <summary>
/// A withdrawal is recorded in the stream of its own level, so it removes the grant of that level only.
/// </summary>
public class a_withdrawal_at_each_level : a_guest_list_inbox
{
    async Task Because()
    {
        await Delivered(Guests.WithdrawFromTenant(Guests.Plus));
        await Delivered(Guests.WithdrawFromUser(Guests.Plus));
        await Delivered(Guests.WithdrawFromSystem(Guests.Plus));
        await ReadRecorded();
    }

    [Fact] void should_record_the_tenants_withdrawal_in_the_tenants_stream() => _recordedForAcme.Count(_ => _.Content is AudienceWithdrawalRecorded { Level: AudienceLevel.Tenant } && _.Context.EventSourceId == GuestListStreams.Tenant).ShouldEqual(1);
    [Fact] void should_record_the_users_withdrawal_in_the_users_stream() => _recordedForAcme.Count(_ => _.Content is AudienceWithdrawalRecorded { Level: AudienceLevel.User } && _.Context.EventSourceId == GuestListStreams.User(Guests.Alice)).ShouldEqual(1);
    [Fact] void should_record_the_systems_withdrawal_in_the_systems_stream() => _recordedForTheProduct.Count(_ => _.Content is AudienceWithdrawalRecorded { Level: AudienceLevel.System } && _.Context.EventSourceId == GuestListStreams.System).ShouldEqual(1);
}
#endif
