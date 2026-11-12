// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.GuestList.Authoring.given;

namespace Cratis.Showrunner.GuestList.Authoring.WithdrawAudience.when_withdrawing_an_audience;

/// <summary>
/// Withdrawing at the level of a user, and at the level of the system, each records its own decision.
/// </summary>
public class a_user_and_the_system : a_guest_list_command<WithdrawAudience>
{
    CommandResult _fromTheSystem;

    void Establish() => Given(Guests.UserGrant, Guests.SystemGrant);

    async Task Because()
    {
        _result = await _scenario.Execute(new WithdrawAudience(Guests.Product, AudienceLevel.User, Guests.Acme, Guests.Alice, Guests.Plus));
        _fromTheSystem = await _scenario.Execute(new WithdrawAudience(Guests.Product, AudienceLevel.System, TenantId.NotSet, UserId.NotSet, Guests.Plus));
    }

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact] void should_succeed_for_the_system() => _fromTheSystem.ShouldBeSuccessful();

    [Fact]
    async Task should_append_the_users_withdrawal() =>
        await _scenario.ShouldHaveAppendedEvent<WithdrawAudience, AudienceWithdrawalDecided>(new EventSourceId(Guests.Product.Value), e => e.Level == AudienceLevel.User && e.UserId == Guests.Alice);

    [Fact]
    async Task should_append_the_systems_withdrawal() =>
        await _scenario.ShouldHaveAppendedEvent<WithdrawAudience, AudienceWithdrawalDecided>(new EventSourceId(Guests.Product.Value), e => e.Level == AudienceLevel.System);
}
#endif
