// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.Authoring.given;

namespace Cratis.Showrunner.GuestList.Authoring.WithdrawAudience.when_withdrawing_an_audience;

/// <summary>
/// A withdrawal is held to the rules of a grant: a valid audience name, the tenant and user its level needs, a tenant the
/// product has.
/// </summary>
public class an_audience_that_names_the_wrong_things : a_guest_list_command<WithdrawAudience>
{
    CommandResult _invalidName;
    CommandResult _userWithoutUser;
    CommandResult _unknownTenant;

    void Establish() => Given(Guests.TenantGrant);

    async Task Because()
    {
        _result = await _scenario.Execute(new WithdrawAudience(Guests.Product, AudienceLevel.Tenant, Guests.Acme, UserId.NotSet, "Not valid"));
        _invalidName = _result;
        _userWithoutUser = await _scenario.Execute(new WithdrawAudience(Guests.Product, AudienceLevel.User, Guests.Acme, UserId.NotSet, Guests.Plus));
        _unknownTenant = await _scenario.Execute(new WithdrawAudience(Guests.Product, AudienceLevel.Tenant, "initech", UserId.NotSet, Guests.Plus));
    }

    [Fact] void should_refuse_an_invalid_name() => _invalidName.ShouldNotBeSuccessful();
    [Fact] void should_refuse_a_user_withdrawal_without_a_user() => _userWithoutUser.ShouldNotBeSuccessful();
    [Fact] void should_refuse_a_tenant_the_product_does_not_have() => _unknownTenant.ShouldNotBeSuccessful();
    [Fact] void should_have_validation_errors() => _result.ShouldHaveValidationErrors();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
