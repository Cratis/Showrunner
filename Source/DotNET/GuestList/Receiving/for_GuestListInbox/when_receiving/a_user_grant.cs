// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.given;
using Cratis.Showrunner.GuestList.Receiving.for_GuestListInbox.given;

namespace Cratis.Showrunner.GuestList.Receiving.for_GuestListInbox.when_receiving;

/// <summary>
/// A grant for a user is recorded in the namespace of the user's tenant, in a stream of its own.
/// </summary>
public class a_user_grant : a_guest_list_inbox
{
    async Task Because()
    {
        await Delivered(Guests.GrantToUser(Guests.Preview));
        await ReadRecorded();
    }

    [Fact] void should_record_a_grant() => _recordedForAcme.Single().Content.ShouldEqual(new AudienceGrantRecorded(AudienceLevel.User, Guests.Preview));
    [Fact] void should_record_it_in_the_users_stream() => _recordedForAcme.Single().Context.EventSourceId.ShouldEqual(GuestListStreams.User(Guests.Alice));
    [Fact] void should_record_nothing_in_the_products_own_namespace() => _recordedForTheProduct.ShouldBeEmpty();
}
#endif
