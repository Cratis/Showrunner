// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Product.Provisioning.for_TenantRegistrations.given;

namespace Cratis.Showrunner.BoxOffice.Product.Provisioning.for_TenantRegistrations.when_registering;

public class a_tenant_the_product_refuses : a_tenant_registration
{
    async Task Because()
    {
        _provisioner.Provision(Arg.Any<TenantToProvision>(), Arg.Any<CancellationToken>()).Returns(TenantProvisioning.Refused("The name is taken"));
        await _registrations.Registered(Registered, _context);
        await Read();
    }

    [Fact] void should_report_why_it_did_not_create_the_tenant() => _outbox.Single().Content.ShouldEqual(new TenantNotProvisioned(Tenants.Tenant, "The name is taken"));
    [Fact] void should_not_report_the_tenant_ready() => _outbox.Select(_ => _.Content).OfType<TenantProvisioned>().ShouldBeEmpty();
    [Fact] void should_record_nothing_in_the_tenant() => _tenantEvents.ShouldBeEmpty();
}
#endif
