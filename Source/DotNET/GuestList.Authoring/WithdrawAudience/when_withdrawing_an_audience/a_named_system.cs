// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.GuestList.Authoring.given;

namespace Cratis.Showrunner.GuestList.Authoring.WithdrawAudience.when_withdrawing_an_audience;

/// <summary>
/// Withdrawing an audience from a named system removes that system's grant, and is recorded with its client id.
/// </summary>
public class a_named_system : a_guest_list_command<WithdrawAudience>
{
    void Establish() => Given(new AudienceGrantDecided(AudienceLevel.NamedSystem, TenantId.NotSet, UserId.NotSet, Guests.Plus, Guests.Crm, SystemKind.Mcp));

    async Task Because() => _result = await _scenario.Execute(new WithdrawAudience(Guests.Product, AudienceLevel.NamedSystem, TenantId.NotSet, UserId.NotSet, Guests.Plus, Guests.Crm, SystemKind.Mcp));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_that_it_was_withdrawn_from_that_system() =>
        await _scenario.ShouldHaveAppendedEvent<WithdrawAudience, AudienceWithdrawalDecided>(
            new EventSourceId(Guests.Product.Value),
            e => e.Level == AudienceLevel.NamedSystem && e.ClientId == Guests.Crm && e.Audience == Guests.Plus);
}
#endif
