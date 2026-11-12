// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.given;
using Cratis.Showrunner.GuestList.Receiving.for_GuestListInbox.given;

namespace Cratis.Showrunner.GuestList.Receiving.for_GuestListInbox.when_receiving;

/// <summary>
/// A grant is a fact: an audience the application does not declare is recorded like any other, so declaring it later makes
/// the old grant live again.
/// </summary>
public class a_grant_for_an_audience_the_application_does_not_declare : a_guest_list_inbox
{
    async Task Because()
    {
        await Delivered(Guests.GrantToTenant("NoSuchAudience"));
        await ReadRecorded();
    }

    [Fact] void should_record_it() => _recordedForAcme.Single().Content.ShouldEqual(new AudienceGrantRecorded(AudienceLevel.Tenant, "NoSuchAudience"));
}
#endif
