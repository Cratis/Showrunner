// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.given;
using Cratis.Showrunner.GuestList.Receiving.for_GuestListInbox.given;

namespace Cratis.Showrunner.GuestList.Receiving.for_GuestListInbox.when_receiving;

/// <summary>
/// A decision about one named system is recorded in that system's own stream in the product's own namespace, apart from the
/// stream of every system and the stream of another system, so it never touches either.
/// </summary>
public class a_named_system_grant_and_withdrawal : a_guest_list_inbox
{
    async Task Because()
    {
        await Delivered(Guests.GrantToNamedSystem(Guests.Plus));
        await Delivered(Guests.GrantToNamedSystem(Guests.Plus, Guests.Reports));
        await Delivered(Guests.WithdrawFromNamedSystem(Guests.Plus));
        await ReadRecorded();
    }

    [Fact] void should_record_the_grant_in_the_stream_of_the_system() => _recordedForTheProduct.Count(_ => _.Content is AudienceGrantRecorded { Level: AudienceLevel.NamedSystem } && _.Context.EventSourceId == GuestListStreams.NamedSystem(Guests.Crm)).ShouldEqual(1);
    [Fact] void should_record_the_grant_of_the_other_system_in_its_own_stream() => _recordedForTheProduct.Count(_ => _.Content is AudienceGrantRecorded { Level: AudienceLevel.NamedSystem } && _.Context.EventSourceId == GuestListStreams.NamedSystem(Guests.Reports)).ShouldEqual(1);
    [Fact] void should_record_the_withdrawal_in_the_stream_of_the_system() => _recordedForTheProduct.Count(_ => _.Content is AudienceWithdrawalRecorded { Level: AudienceLevel.NamedSystem } && _.Context.EventSourceId == GuestListStreams.NamedSystem(Guests.Crm)).ShouldEqual(1);
    [Fact] void should_record_nothing_in_the_stream_of_every_system() => _recordedForTheProduct.Count(_ => _.Context.EventSourceId == GuestListStreams.System).ShouldEqual(0);
    [Fact] void should_record_nothing_for_the_tenant() => _recordedForAcme.ShouldBeEmpty();
}
#endif
