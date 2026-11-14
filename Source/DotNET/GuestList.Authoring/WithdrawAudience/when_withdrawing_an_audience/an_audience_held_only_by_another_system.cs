// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.Authoring.given;

namespace Cratis.Showrunner.GuestList.Authoring.WithdrawAudience.when_withdrawing_an_audience;

/// <summary>
/// A withdrawal removes the grant of its own holder only: withdrawing an audience from a system that does not hold it records
/// nothing when every system or another system holds it.
/// </summary>
public class an_audience_held_only_by_another_system : a_guest_list_command<WithdrawAudience>
{
    CommandResult _fromTheSystem;

    void Establish() => Given(Guests.SystemGrant, new AudienceGrantDecided(AudienceLevel.NamedSystem, TenantId.NotSet, UserId.NotSet, Guests.Plus, Guests.Reports, SystemKind.Api));

    async Task Because()
    {
        _result = await _scenario.Execute(new WithdrawAudience(Guests.Product, AudienceLevel.NamedSystem, TenantId.NotSet, UserId.NotSet, Guests.Plus, Guests.Crm, SystemKind.Mcp));
        _fromTheSystem = await _scenario.Execute(new WithdrawAudience(Guests.Product, AudienceLevel.NamedSystem, TenantId.NotSet, UserId.NotSet, Guests.Preview, Guests.Reports, SystemKind.Api));
    }

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();
    [Fact] void should_succeed_for_an_audience_the_system_does_not_hold() => _fromTheSystem.ShouldBeSuccessful();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
