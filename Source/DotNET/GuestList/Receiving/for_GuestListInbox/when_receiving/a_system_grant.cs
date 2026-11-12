// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.given;
using Cratis.Showrunner.GuestList.Receiving.for_GuestListInbox.given;

namespace Cratis.Showrunner.GuestList.Receiving.for_GuestListInbox.when_receiving;

/// <summary>
/// A grant for the system is recorded in the product's own namespace, where the operator published it, in the system's stream.
/// </summary>
public class a_system_grant : a_guest_list_inbox
{
    async Task Because()
    {
        await Delivered(Guests.GrantToSystem(Guests.Plus));
        await ReadRecorded();
    }

    [Fact] void should_record_a_grant() => _recordedForTheProduct.Single().Content.ShouldEqual(new AudienceGrantRecorded(AudienceLevel.System, Guests.Plus));
    [Fact] void should_record_it_in_the_systems_stream() => _recordedForTheProduct.Single().Context.EventSourceId.ShouldEqual(GuestListStreams.System);
    [Fact] void should_record_nothing_for_the_tenant() => _recordedForAcme.ShouldBeEmpty();
}
#endif
