// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Choreography.Availability.Audiences;
using Cratis.Showrunner.GuestList.Availability.given;
using SystemClientId = Cratis.Showrunner.GuestList.Contracts.Audiences.SystemClientId;

namespace Cratis.Showrunner.GuestList.Availability.for_GuestListAudienceSource;

/// <summary>
/// A system subject that carries its client id through the engine gets the grants every system holds and its own. One with
/// no client id gets the ones every system holds only, and a person never gets a system's own grants.
/// </summary>
public class when_a_system_subject_names_its_client_id : a_source_over_a_guest_list
{
    static readonly SystemClientId Crm = new("crm-sync");

    IReadOnlyCollection<AudienceGrant> _forCrm;
    IReadOnlyCollection<AudienceGrant> _forNoClient;
    IReadOnlyCollection<AudienceGrant> _forUnknownClient;
    IReadOnlyCollection<AudienceGrant> _forAPersonNamingAClient;

    void Establish()
    {
        Holds(system: ["Internal"], tenant: ["Plus"], user: []);
        HoldsForSystem(Crm, system: ["Internal"], named: ["Preview"]);
    }

    async Task Because()
    {
        _forCrm = await _source.GetGrants(new(null, null, true) { SystemClientId = "crm-sync" });
        _forNoClient = await _source.GetGrants(new(null, null, true));
        _forUnknownClient = await _source.GetGrants(new(null, null, true) { SystemClientId = "unknown" });
        _forAPersonNamingAClient = await _source.GetGrants(new("acme", null) { SystemClientId = "crm-sync" });
    }

    [Fact] void should_add_the_systems_own_grants_to_the_ones_every_system_holds() =>
        _forCrm.ShouldContainOnly(
            new AudienceGrant("Internal", AudienceLevel.System),
            new AudienceGrant("Preview", AudienceLevel.System));

    [Fact] void should_give_a_subject_with_no_client_id_only_what_every_system_holds() => _forNoClient.ShouldContainOnly(new AudienceGrant("Internal", AudienceLevel.System));
    [Fact] void should_give_an_unknown_client_id_only_what_every_system_holds() => _forUnknownClient.ShouldContainOnly(new AudienceGrant("Internal", AudienceLevel.System));

    [Fact] void should_not_give_a_person_the_grants_of_a_system() =>
        _forAPersonNamingAClient.ShouldContainOnly(
            new AudienceGrant("Internal", AudienceLevel.System),
            new AudienceGrant("Plus", AudienceLevel.Tenant));
}
#endif
