// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.Authoring.given;

namespace Cratis.Showrunner.GuestList.Authoring.GrantAudience.when_granting_an_audience;

/// <summary>
/// Each level names exactly what it needs: a tenant grant a tenant and no user, a user grant a tenant and a user, a system
/// grant neither. Anything else is refused, so the same decision is never written two ways.
/// </summary>
public class a_level_that_names_the_wrong_things : a_guest_list_command<GrantAudience>
{
    CommandResult _tenantWithoutTenant;
    CommandResult _tenantNamingAUser;
    CommandResult _userWithoutUser;
    CommandResult _userWithoutTenant;
    CommandResult _systemNamingATenant;
    CommandResult _systemNamingAUser;
    CommandResult _noProduct;
    CommandResult _unknownLevel;

    async Task Because()
    {
        _tenantWithoutTenant = await _scenario.Execute(new GrantAudience(Guests.Product, AudienceLevel.Tenant, TenantId.NotSet, UserId.NotSet, Guests.Plus));
        _tenantNamingAUser = await _scenario.Execute(new GrantAudience(Guests.Product, AudienceLevel.Tenant, Guests.Acme, Guests.Alice, Guests.Plus));
        _userWithoutUser = await _scenario.Execute(new GrantAudience(Guests.Product, AudienceLevel.User, Guests.Acme, UserId.NotSet, Guests.Plus));
        _userWithoutTenant = await _scenario.Execute(new GrantAudience(Guests.Product, AudienceLevel.User, TenantId.NotSet, Guests.Alice, Guests.Plus));
        _systemNamingATenant = await _scenario.Execute(new GrantAudience(Guests.Product, AudienceLevel.System, Guests.Acme, UserId.NotSet, Guests.Plus));
        _systemNamingAUser = await _scenario.Execute(new GrantAudience(Guests.Product, AudienceLevel.System, TenantId.NotSet, Guests.Alice, Guests.Plus));
        _noProduct = await _scenario.Execute(new GrantAudience(Product.NotSet, AudienceLevel.System, TenantId.NotSet, UserId.NotSet, Guests.Plus));
        _unknownLevel = await _scenario.Execute(new GrantAudience(Guests.Product, (AudienceLevel)42, TenantId.NotSet, UserId.NotSet, Guests.Plus));
    }

    [Fact] void should_refuse_a_tenant_grant_without_a_tenant() => _tenantWithoutTenant.ShouldNotBeSuccessful();
    [Fact] void should_refuse_a_tenant_grant_naming_a_user() => _tenantNamingAUser.ShouldNotBeSuccessful();
    [Fact] void should_refuse_a_user_grant_without_a_user() => _userWithoutUser.ShouldNotBeSuccessful();
    [Fact] void should_refuse_a_user_grant_without_a_tenant() => _userWithoutTenant.ShouldNotBeSuccessful();
    [Fact] void should_refuse_a_system_grant_naming_a_tenant() => _systemNamingATenant.ShouldNotBeSuccessful();
    [Fact] void should_refuse_a_system_grant_naming_a_user() => _systemNamingAUser.ShouldNotBeSuccessful();
    [Fact] void should_refuse_a_grant_for_no_product() => _noProduct.ShouldNotBeSuccessful();
    [Fact] void should_refuse_a_level_that_does_not_exist() => _unknownLevel.ShouldNotBeSuccessful();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
