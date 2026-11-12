// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.given;
using Cratis.Showrunner.GuestList.Querying.for_GuestLists.given;

namespace Cratis.Showrunner.GuestList.Querying.for_GuestLists.when_asking;

/// <summary>
/// The audiences of a request are the tenant's, the user's and the system's, added together. Each level keeps its own
/// grants: withdrawing at one level leaves the grants at the others in place.
/// </summary>
public class for_the_audiences_of_a_request : a_guest_list
{
    IReadOnlySet<AudienceName> _forAlice;
    IReadOnlySet<AudienceName> _forBob;
    IReadOnlySet<AudienceName> _forNoUser;
    IReadOnlySet<AudienceName> _forTheTenant;
    IReadOnlySet<AudienceName> _forAliceAlone;
    IReadOnlySet<AudienceName> _forTheSystem;

    void Establish()
    {
        SystemRecorded((0, new AudienceGrantRecorded(AudienceLevel.System, Guests.Internal)));
        TenantRecorded(
            (1, new AudienceGrantRecorded(AudienceLevel.Tenant, Guests.Plus)),
            (2, new AudienceGrantRecorded(AudienceLevel.Tenant, Guests.Preview)),
            (3, new AudienceWithdrawalRecorded(AudienceLevel.Tenant, Guests.Preview)));
        UserRecorded(
            Guests.Alice,
            (4, new AudienceGrantRecorded(AudienceLevel.User, Guests.Preview)),
            (5, new AudienceGrantRecorded(AudienceLevel.User, Guests.Plus)));
    }

    async Task Because()
    {
        _forAlice = await _guestList.For(Guests.Acme, Guests.Alice);
        _forBob = await _guestList.For(Guests.Acme, Guests.Bob);
        _forNoUser = await _guestList.For(Guests.Acme, UserId.NotSet);
        _forTheTenant = await _guestList.ForTenant(Guests.Acme);
        _forAliceAlone = await _guestList.ForUser(Guests.Acme, Guests.Alice);
        _forTheSystem = await _guestList.ForSystem();
    }

    [Fact] void should_add_the_three_levels_for_a_user() => _forAlice.ShouldContainOnly(Guests.Internal, Guests.Plus, Guests.Preview);
    [Fact] void should_leave_out_what_another_user_holds() => _forBob.ShouldContainOnly(Guests.Internal, Guests.Plus);
    [Fact] void should_add_the_tenant_and_the_system_for_a_request_that_is_not_a_person() => _forNoUser.ShouldContainOnly(Guests.Internal, Guests.Plus);
    [Fact] void should_answer_the_tenants_own_audiences() => _forTheTenant.ShouldContainOnly(Guests.Plus);
    [Fact] void should_answer_the_users_own_audiences() => _forAliceAlone.ShouldContainOnly(Guests.Preview, Guests.Plus);
    [Fact] void should_answer_the_systems_own_audiences() => _forTheSystem.ShouldContainOnly(Guests.Internal);
}
#endif
