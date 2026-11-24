// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.Authoring.given;

namespace Cratis.Showrunner.GuestList.Authoring.SuspendPart.when_suspending_a_part;

/// <summary>
/// Each level names exactly what it needs, the product and the part are named, the part is a path, and a tenant the product
/// does not have is refused.
/// </summary>
public class a_level_that_names_the_wrong_things : a_guest_list_command<SuspendPart>
{
    CommandResult _tenantWithoutTenant;
    CommandResult _tenantNamingAUser;
    CommandResult _userWithoutUser;
    CommandResult _systemNamingATenant;
    CommandResult _unknownTenant;
    CommandResult _noPart;
    CommandResult _malformedPart;
    CommandResult _noProduct;
    CommandResult _unknownLevel;

    async Task Because()
    {
        _tenantWithoutTenant = await _scenario.Execute(new SuspendPart(Guests.Product, SuspensionLevel.Tenant, TenantId.NotSet, UserId.NotSet, Guests.FleetReports));
        _tenantNamingAUser = await _scenario.Execute(new SuspendPart(Guests.Product, SuspensionLevel.Tenant, Guests.Acme, Guests.Alice, Guests.FleetReports));
        _userWithoutUser = await _scenario.Execute(new SuspendPart(Guests.Product, SuspensionLevel.User, Guests.Acme, UserId.NotSet, Guests.FleetReports));
        _systemNamingATenant = await _scenario.Execute(new SuspendPart(Guests.Product, SuspensionLevel.System, Guests.Acme, UserId.NotSet, Guests.FleetReports));
        _unknownTenant = await _scenario.Execute(new SuspendPart(Guests.Product, SuspensionLevel.Tenant, "initech", UserId.NotSet, Guests.FleetReports));
        _noPart = await _scenario.Execute(new SuspendPart(Guests.Product, SuspensionLevel.System, TenantId.NotSet, UserId.NotSet, PartPath.NotSet));
        _malformedPart = await _scenario.Execute(new SuspendPart(Guests.Product, SuspensionLevel.System, TenantId.NotSet, UserId.NotSet, "Fleet//Reports"));
        _noProduct = await _scenario.Execute(new SuspendPart(Product.NotSet, SuspensionLevel.System, TenantId.NotSet, UserId.NotSet, Guests.FleetReports));
        _unknownLevel = await _scenario.Execute(new SuspendPart(Guests.Product, (SuspensionLevel)42, TenantId.NotSet, UserId.NotSet, Guests.FleetReports));
    }

    [Fact] void should_refuse_a_tenant_suspension_without_a_tenant() => _tenantWithoutTenant.ShouldNotBeSuccessful();
    [Fact] void should_refuse_a_tenant_suspension_naming_a_user() => _tenantNamingAUser.ShouldNotBeSuccessful();
    [Fact] void should_refuse_a_user_suspension_without_a_user() => _userWithoutUser.ShouldNotBeSuccessful();
    [Fact] void should_refuse_a_system_suspension_naming_a_tenant() => _systemNamingATenant.ShouldNotBeSuccessful();
    [Fact] void should_refuse_a_tenant_the_product_does_not_have() => _unknownTenant.ShouldNotBeSuccessful();
    [Fact] void should_refuse_no_part() => _noPart.ShouldNotBeSuccessful();
    [Fact] void should_refuse_a_part_that_is_not_a_path() => _malformedPart.ShouldNotBeSuccessful();
    [Fact] void should_refuse_no_product() => _noProduct.ShouldNotBeSuccessful();
    [Fact] void should_refuse_an_unknown_level() => _unknownLevel.ShouldNotBeSuccessful();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
