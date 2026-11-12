// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Choreography.Availability.Audiences;
using Cratis.Showrunner.GuestList.Availability.given;

namespace Cratis.Showrunner.GuestList.Availability.for_GuestListAudienceSource;

/// <summary>
/// The grants of a request are the tenant's, the user's and the system's, each tagged with the level it is held at.
/// </summary>
public class when_getting_grants : a_source_over_a_guest_list
{
    IReadOnlyCollection<AudienceGrant> _forAlice;
    IReadOnlyCollection<AudienceGrant> _forTheTenant;
    IReadOnlyCollection<AudienceGrant> _forNoTenant;
    IReadOnlyCollection<AudienceGrant> _forAUserWithoutATenant;

    void Establish() => Holds(system: ["Internal", "Plus"], tenant: ["Plus"], user: ["Preview", "Plus"]);

    async Task Because()
    {
        _forAlice = await _source.GetGrants(new("acme", "alice"));
        _forTheTenant = await _source.GetGrants(new("acme", null));
        _forNoTenant = await _source.GetGrants(new(null, null, IsSystem: true));
        _forAUserWithoutATenant = await _source.GetGrants(new(null, "alice"));
    }

    [Fact] void should_add_the_three_levels_for_a_user() =>
        _forAlice.ShouldContainOnly(
            new AudienceGrant("Internal", AudienceLevel.System),
            new AudienceGrant("Plus", AudienceLevel.System),
            new AudienceGrant("Plus", AudienceLevel.Tenant),
            new AudienceGrant("Preview", AudienceLevel.User),
            new AudienceGrant("Plus", AudienceLevel.User));

    [Fact] void should_keep_an_audience_held_at_two_levels_at_both() => _forAlice.Count(_ => _.Audience == "Plus").ShouldEqual(3);

    [Fact] void should_leave_the_user_out_for_a_request_without_one() =>
        _forTheTenant.ShouldContainOnly(
            new AudienceGrant("Internal", AudienceLevel.System),
            new AudienceGrant("Plus", AudienceLevel.System),
            new AudienceGrant("Plus", AudienceLevel.Tenant));

    [Fact] void should_answer_the_systems_for_a_request_without_a_tenant() =>
        _forNoTenant.ShouldContainOnly(
            new AudienceGrant("Internal", AudienceLevel.System),
            new AudienceGrant("Plus", AudienceLevel.System));

    [Fact] void should_not_look_for_a_user_without_a_tenant() => _forAUserWithoutATenant.Count.ShouldEqual(2);
}
#endif
