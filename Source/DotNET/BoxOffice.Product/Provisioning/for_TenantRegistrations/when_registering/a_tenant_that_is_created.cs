// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Product.Provisioning.for_TenantRegistrations.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Product.Provisioning.for_TenantRegistrations.when_registering;

public class a_tenant_that_is_created : a_tenant_registration
{
    async Task Because()
    {
        await _registrations.Registered(Registered, _context);
        await Read();
    }

    [Fact] void should_ask_the_product_to_create_the_tenant() => _provisioner.Received(1).Provision(Arg.Is<TenantToProvision>(_ => _.Tenant == Tenants.Tenant && _.Creator == Tenants.Ada && _.Name == (TenantName)"Fjord Logistics" && _.CreatorRole == (TenantRole)"Maintainer"), Arg.Any<CancellationToken>());
    [Fact] void should_report_the_tenant_ready() => _outbox.Single().Content.ShouldEqual(new TenantProvisioned(Tenants.Tenant));
    [Fact] void should_report_under_the_operators_organization() => _outbox.Single().Context.EventSourceId.Value.ShouldEqual(Tenants.Organization.Value.ToString("D"));
    [Fact] void should_tell_the_tenant_which_organization_it_is() => _tenantEvents.Select(_ => _.Content).OfType<TenantOrganizationIdentified>().Single().Organization.ShouldEqual(Tenants.Organization);
    [Fact] void should_remember_who_created_the_tenant() => _tenantEvents.Select(_ => _.Content).OfType<TenantCreatorIdentified>().Single().Creator.ShouldEqual(Tenants.Ada);
}
#endif
