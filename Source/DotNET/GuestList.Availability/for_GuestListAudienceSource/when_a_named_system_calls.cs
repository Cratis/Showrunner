// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Choreography.Availability.Audiences;
using Cratis.Showrunner.GuestList.Availability.given;
using SystemClientId = Cratis.Showrunner.GuestList.Contracts.Audiences.SystemClientId;

namespace Cratis.Showrunner.GuestList.Availability.for_GuestListAudienceSource;

/// <summary>
/// A system that names its client id gets the grants every system holds and its own, at the level of the system, besides
/// the tenant's. Another system's grants are never part of the answer, and a caller that names no client id gets the ones
/// every system holds only.
/// </summary>
public class when_a_named_system_calls : a_source_over_a_guest_list
{
    static readonly SystemClientId Crm = new("crm-sync");
    static readonly SystemClientId Reports = new("reports-mcp");

    IReadOnlyCollection<AudienceGrant> _forCrm;
    IReadOnlyCollection<AudienceGrant> _forCrmOfATenant;
    IReadOnlyCollection<AudienceGrant> _forReports;
    IReadOnlyCollection<AudienceGrant> _forNoClient;

    void Establish()
    {
        Holds(system: ["Internal"], tenant: ["Plus"], user: []);
        HoldsForSystem(Crm, system: ["Internal"], named: ["Preview"]);
        HoldsForSystem(Reports, system: ["Internal"], named: []);
    }

    async Task Because()
    {
        _forCrm = await _source.GetGrants(new(null, null, IsSystem: true), Crm);
        _forCrmOfATenant = await _source.GetGrants(new("acme", null, IsSystem: true), Crm);
        _forReports = await _source.GetGrants(new(null, null, IsSystem: true), Reports);
        _forNoClient = await _source.GetGrants(new(null, null, IsSystem: true), SystemClientId.NotSet);
    }

    [Fact] void should_add_the_systems_own_grants_to_the_ones_every_system_holds() =>
        _forCrm.ShouldContainOnly(
            new AudienceGrant("Internal", AudienceLevel.System),
            new AudienceGrant("Preview", AudienceLevel.System));

    [Fact] void should_keep_the_tenant_of_the_request() =>
        _forCrmOfATenant.ShouldContainOnly(
            new AudienceGrant("Internal", AudienceLevel.System),
            new AudienceGrant("Preview", AudienceLevel.System),
            new AudienceGrant("Plus", AudienceLevel.Tenant));

    [Fact] void should_not_give_a_system_what_another_system_holds() => _forReports.ShouldContainOnly(new AudienceGrant("Internal", AudienceLevel.System));
    [Fact] void should_give_a_caller_with_no_client_id_only_what_every_system_holds() => _forNoClient.ShouldContainOnly(new AudienceGrant("Internal", AudienceLevel.System));
}
#endif
