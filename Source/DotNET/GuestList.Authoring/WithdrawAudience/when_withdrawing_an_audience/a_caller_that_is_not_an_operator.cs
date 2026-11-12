// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Showrunner.GuestList.Authoring.given;

namespace Cratis.Showrunner.GuestList.Authoring.WithdrawAudience.when_withdrawing_an_audience;

/// <summary>
/// A caller the host's authoring policy does not admit may not withdraw.
/// </summary>
public class a_caller_that_is_not_an_operator : a_guest_list_command<WithdrawAudience>
{
    protected override ClaimsPrincipal Principal => Operators.Somebody;

    void Establish() => Given(Guests.TenantGrant);

    async Task Because() => _result = await _scenario.Execute(new WithdrawAudience(Guests.Product, AudienceLevel.Tenant, Guests.Acme, UserId.NotSet, Guests.Plus));

    [Fact] void should_not_be_authorized() => _result.IsAuthorized.ShouldBeFalse();

    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
