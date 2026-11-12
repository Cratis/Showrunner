// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.given;
using Cratis.Showrunner.GuestList.Querying.for_GuestLists.given;

namespace Cratis.Showrunner.GuestList.Querying.for_GuestLists.when_asking;

/// <summary>
/// The grants of a tenant are its own, those of each of its users, and the system's, with the level of each.
/// </summary>
public class for_the_grants_of_a_tenant : a_guest_list
{
    IReadOnlyList<GrantedAudience> _granted;

    void Establish()
    {
        SystemRecorded((0, new AudienceGrantRecorded(AudienceLevel.System, Guests.Internal)));
        TenantRecorded((1, new AudienceGrantRecorded(AudienceLevel.Tenant, Guests.Plus)));
        UserRecorded(Guests.Alice, (2, new AudienceGrantRecorded(AudienceLevel.User, Guests.Preview)));
        UserRecorded(Guests.Bob, (3, new AudienceGrantRecorded(AudienceLevel.User, Guests.Plus)), (4, new AudienceWithdrawalRecorded(AudienceLevel.User, Guests.Plus)));
    }

    async Task Because() => _granted = await _guestList.Granted(Guests.Acme);

    [Fact] void should_list_the_tenants_grant() => _granted.ShouldContain(new GrantedAudience(AudienceLevel.Tenant, UserId.NotSet, Guests.Plus));
    [Fact] void should_list_the_users_grant() => _granted.ShouldContain(new GrantedAudience(AudienceLevel.User, Guests.Alice, Guests.Preview));
    [Fact] void should_list_the_systems_grant() => _granted.ShouldContain(new GrantedAudience(AudienceLevel.System, UserId.NotSet, Guests.Internal));
    [Fact] void should_not_list_a_grant_that_was_withdrawn() => _granted.Count.ShouldEqual(3);
}
#endif
