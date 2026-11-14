// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.given;
using Cratis.Showrunner.GuestList.Querying.for_GuestLists.given;

namespace Cratis.Showrunner.GuestList.Querying.for_GuestLists.when_asking;

/// <summary>
/// The audiences of a request made by a named system are the ones every system holds plus its own, never another system's: a
/// system the operator granted nothing gets only the ones every system holds, and so does a caller that names no client id.
/// </summary>
public class for_the_audiences_of_a_system_caller : a_guest_list
{
    IReadOnlySet<AudienceName> _forCrm;
    IReadOnlySet<AudienceName> _forReports;
    IReadOnlySet<AudienceName> _forNoClient;
    IReadOnlySet<AudienceName> _forCrmOnly;

    void Establish()
    {
        SystemRecorded((1, new AudienceGrantRecorded(AudienceLevel.System, Guests.Internal)));
        NamedSystemRecorded(Guests.Crm, (2, new AudienceGrantRecorded(AudienceLevel.NamedSystem, Guests.Plus)), (4, new AudienceGrantRecorded(AudienceLevel.NamedSystem, Guests.Preview)), (5, new AudienceWithdrawalRecorded(AudienceLevel.NamedSystem, Guests.Preview)));
        NamedSystemRecorded(Guests.Reports, (3, new AudienceGrantRecorded(AudienceLevel.NamedSystem, Guests.Preview)));
    }

    async Task Because()
    {
        _forCrm = await _guestList.ForSystemCaller(Guests.Crm);
        _forReports = await _guestList.ForSystemCaller(Guests.Reports);
        _forNoClient = await _guestList.ForSystemCaller(SystemClientId.NotSet);
        _forCrmOnly = await _guestList.ForNamedSystem(Guests.Crm);
    }

    [Fact] void should_give_a_system_what_every_system_holds_and_its_own() => _forCrm.ShouldContainOnly(Guests.Internal, Guests.Plus);
    [Fact] void should_not_give_a_system_what_another_system_holds() => _forReports.ShouldContainOnly(Guests.Internal, Guests.Preview);
    [Fact] void should_give_a_caller_with_no_client_id_only_what_every_system_holds() => _forNoClient.ShouldContainOnly(Guests.Internal);
    [Fact] void should_leave_what_every_system_holds_out_of_the_systems_own() => _forCrmOnly.ShouldContainOnly(Guests.Plus);
}
#endif
