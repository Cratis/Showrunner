// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Product.Provisioning.for_TenantRegistrations.given;

namespace Cratis.Showrunner.BoxOffice.Product.Provisioning.for_TenantRegistrations.when_closing;

public class a_product_that_takes_part : a_tenant_registration
{
    async Task Because() => await _registrations.Closing(new TenantClosing(Tenants.Tenant));

    [Fact] void should_tell_the_product_the_tenant_is_closing() => _closing.Received(1).Closing(Tenants.Tenant, Arg.Any<CancellationToken>());
}
#endif
