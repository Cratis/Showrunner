// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.GuestList.Authoring.given;

namespace Cratis.Showrunner.GuestList.Authoring.WithdrawAudience.when_withdrawing_an_audience;

/// <summary>
/// Withdrawing an audience that is held at that level records the decision in the product's stream.
/// </summary>
public class an_audience_that_is_held : a_guest_list_command<WithdrawAudience>
{
    void Establish() => Given(Guests.TenantGrant);

    async Task Because() => _result = await _scenario.Execute(new WithdrawAudience(Guests.Product, AudienceLevel.Tenant, Guests.Acme, UserId.NotSet, Guests.Plus));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_that_it_was_withdrawn() =>
        await _scenario.ShouldHaveAppendedEvent<WithdrawAudience, AudienceWithdrawalDecided>(
            new EventSourceId(Guests.Product.Value),
            e => e.Level == AudienceLevel.Tenant && e.TenantId == Guests.Acme && e.UserId == UserId.NotSet && e.Audience == Guests.Plus);
}
#endif
