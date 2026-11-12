// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.given;
using Cratis.Showrunner.GuestList.Querying.for_GuestLists.given;

namespace Cratis.Showrunner.GuestList.Querying.for_GuestLists.when_asking;

/// <summary>
/// An audience granted at the tenant and to a user stays with the user when the tenant's grant is withdrawn: a withdrawal
/// removes the grant of its own level only.
/// </summary>
public class for_an_audience_withdrawn_at_one_level_only : a_guest_list
{
    IReadOnlySet<AudienceName> _forAlice;
    IReadOnlySet<AudienceName> _forBob;

    void Establish()
    {
        TenantRecorded(
            (1, new AudienceGrantRecorded(AudienceLevel.Tenant, Guests.Plus)),
            (3, new AudienceWithdrawalRecorded(AudienceLevel.Tenant, Guests.Plus)));
        UserRecorded(Guests.Alice, (2, new AudienceGrantRecorded(AudienceLevel.User, Guests.Plus)));
    }

    async Task Because()
    {
        _forAlice = await _guestList.For(Guests.Acme, Guests.Alice);
        _forBob = await _guestList.For(Guests.Acme, Guests.Bob);
    }

    [Fact] void should_keep_the_audience_for_the_user_who_was_granted_it() => _forAlice.ShouldContainOnly(Guests.Plus);
    [Fact] void should_not_have_the_audience_for_anybody_else() => _forBob.ShouldBeEmpty();
}
#endif
