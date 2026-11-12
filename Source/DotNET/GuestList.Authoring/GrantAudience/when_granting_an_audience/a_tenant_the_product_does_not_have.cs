// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.Authoring.given;

namespace Cratis.Showrunner.GuestList.Authoring.GrantAudience.when_granting_an_audience;

/// <summary>
/// A grant for a tenant, or for a user of a tenant, that the product does not have is refused where the operator types it.
/// </summary>
public class a_tenant_the_product_does_not_have : a_guest_list_command<GrantAudience>
{
    CommandResult _forUser;

    async Task Because()
    {
        _result = await _scenario.Execute(new GrantAudience(Guests.Product, AudienceLevel.Tenant, "initech", UserId.NotSet, Guests.Plus));
        _forUser = await _scenario.Execute(new GrantAudience(Guests.Product, AudienceLevel.User, "initech", Guests.Alice, Guests.Plus));
    }

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();

    [Fact] void should_have_validation_errors() => _result.ShouldHaveValidationErrors();

    [Fact] void should_refuse_a_user_of_it_too() => _forUser.ShouldNotBeSuccessful();

    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
