// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.GuestList.Authoring.Tenancy.for_AnyNamedTenant;

/// <summary>
/// Without a register of tenants the operator side knows only whether a tenant is named.
/// </summary>
public class when_asking : Specification
{
    bool _named;
    bool _unnamed;

    async Task Because()
    {
        var tenants = new AnyNamedTenant();
        _named = await tenants.Exists("ledger", "acme");
        _unnamed = await tenants.Exists("ledger", TenantId.NotSet);
    }

    [Fact] void should_accept_a_named_tenant() => _named.ShouldBeTrue();
    [Fact] void should_not_accept_no_tenant() => _unnamed.ShouldBeFalse();
}
#endif
