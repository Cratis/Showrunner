// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.Authoring.Tenancy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.GuestList.Authoring.for_GuestListAuthoringServiceCollectionExtensions;

/// <summary>
/// The authoring side needs nothing from Direct: the defaults address decisions to products and accept any named tenant,
/// and a host's own register of tenants replaces the default.
/// </summary>
public class when_adding_guest_list_authoring : Specification
{
    ServiceProvider _provider;
    ServiceProvider _withTenants;

    void Because()
    {
        _provider = new ServiceCollection().AddGuestListAuthoring().BuildServiceProvider();
        _withTenants = new ServiceCollection().AddSingleton<IKnownTenants>(new TenantsOfTheHost()).AddGuestListAuthoring().BuildServiceProvider();
    }

    [Fact] void should_address_decisions_to_products() => _provider.GetRequiredService<IOptions<GuestListAuthoringOptions>>().Value.AddressToProducts.ShouldBeTrue();
    [Fact] void should_accept_any_named_tenant_by_default() => _provider.GetRequiredService<IKnownTenants>().ShouldBeOfExactType<AnyNamedTenant>();
    [Fact] void should_use_the_hosts_own_register_of_tenants() => _withTenants.GetRequiredService<IKnownTenants>().ShouldBeOfExactType<TenantsOfTheHost>();

    sealed class TenantsOfTheHost : IKnownTenants
    {
        public Task<bool> Exists(Product product, TenantId tenant, CancellationToken cancellationToken = default) => Task.FromResult(false);
    }
}
#endif
