// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.Authoring.given;

namespace Cratis.Showrunner.GuestList.Authoring.GrantAudience.when_granting_an_audience;

/// <summary>
/// A named system names its client id and its kind and nothing else; only a named system names them. Anything else is
/// refused, so a grant to one system is never written as a grant to every system.
/// </summary>
public class a_system_that_is_named_wrongly : a_guest_list_command<GrantAudience>
{
    CommandResult _withoutClientId;
    CommandResult _withAnEmptyClientId;
    CommandResult _withoutKind;
    CommandResult _withAnUnknownKind;
    CommandResult _namingATenant;
    CommandResult _namingAUser;
    CommandResult _systemNamingAClient;
    CommandResult _systemNamingAKind;
    CommandResult _tenantNamingAClient;
    CommandResult _clientIdTooLong;

    async Task Because()
    {
        _withoutClientId = await _scenario.Execute(new GrantAudience(Guests.Product, AudienceLevel.NamedSystem, TenantId.NotSet, UserId.NotSet, Guests.Plus, null, SystemKind.Mcp));
        _withAnEmptyClientId = await _scenario.Execute(new GrantAudience(Guests.Product, AudienceLevel.NamedSystem, TenantId.NotSet, UserId.NotSet, Guests.Plus, SystemClientId.NotSet, SystemKind.Mcp));
        _withoutKind = await _scenario.Execute(new GrantAudience(Guests.Product, AudienceLevel.NamedSystem, TenantId.NotSet, UserId.NotSet, Guests.Plus, Guests.Crm, SystemKind.None));
        _withAnUnknownKind = await _scenario.Execute(new GrantAudience(Guests.Product, AudienceLevel.NamedSystem, TenantId.NotSet, UserId.NotSet, Guests.Plus, Guests.Crm, (SystemKind)42));
        _namingATenant = await _scenario.Execute(new GrantAudience(Guests.Product, AudienceLevel.NamedSystem, Guests.Acme, UserId.NotSet, Guests.Plus, Guests.Crm, SystemKind.Mcp));
        _namingAUser = await _scenario.Execute(new GrantAudience(Guests.Product, AudienceLevel.NamedSystem, TenantId.NotSet, Guests.Alice, Guests.Plus, Guests.Crm, SystemKind.Mcp));
        _systemNamingAClient = await _scenario.Execute(new GrantAudience(Guests.Product, AudienceLevel.System, TenantId.NotSet, UserId.NotSet, Guests.Plus, Guests.Crm, SystemKind.None));
        _systemNamingAKind = await _scenario.Execute(new GrantAudience(Guests.Product, AudienceLevel.System, TenantId.NotSet, UserId.NotSet, Guests.Plus, null, SystemKind.Mcp));
        _tenantNamingAClient = await _scenario.Execute(new GrantAudience(Guests.Product, AudienceLevel.Tenant, Guests.Acme, UserId.NotSet, Guests.Plus, Guests.Crm, SystemKind.None));
        _clientIdTooLong = await _scenario.Execute(new GrantAudience(Guests.Product, AudienceLevel.NamedSystem, TenantId.NotSet, UserId.NotSet, Guests.Plus, new string('c', SystemClientId.MaxLength + 1), SystemKind.Mcp));
    }

    [Fact] void should_refuse_a_system_without_a_client_id() => _withoutClientId.ShouldNotBeSuccessful();
    [Fact] void should_refuse_a_system_with_an_empty_client_id() => _withAnEmptyClientId.ShouldNotBeSuccessful();
    [Fact] void should_refuse_a_system_without_a_kind() => _withoutKind.ShouldNotBeSuccessful();
    [Fact] void should_refuse_a_kind_that_does_not_exist() => _withAnUnknownKind.ShouldNotBeSuccessful();
    [Fact] void should_refuse_a_system_naming_a_tenant() => _namingATenant.ShouldNotBeSuccessful();
    [Fact] void should_refuse_a_system_naming_a_user() => _namingAUser.ShouldNotBeSuccessful();
    [Fact] void should_refuse_a_grant_to_every_system_naming_a_client() => _systemNamingAClient.ShouldNotBeSuccessful();
    [Fact] void should_refuse_a_grant_to_every_system_naming_a_kind() => _systemNamingAKind.ShouldNotBeSuccessful();
    [Fact] void should_refuse_a_tenant_grant_naming_a_client() => _tenantNamingAClient.ShouldNotBeSuccessful();
    [Fact] void should_refuse_a_client_id_that_is_too_long() => _clientIdTooLong.ShouldNotBeSuccessful();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
