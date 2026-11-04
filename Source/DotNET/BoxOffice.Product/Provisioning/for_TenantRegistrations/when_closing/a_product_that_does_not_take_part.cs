// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Product.Provisioning.for_TenantRegistrations.given;

namespace Cratis.Showrunner.BoxOffice.Product.Provisioning.for_TenantRegistrations.when_closing;

public class a_product_that_does_not_take_part : a_tenant_registration
{
    Exception _exception;

    async Task Because()
    {
        var registrations = new TenantRegistrations(_client, _provisioner, Microsoft.Extensions.Logging.Abstractions.NullLogger<TenantRegistrations>.Instance);
        _exception = await Catch.Exception(() => registrations.Closing(new TenantClosing(Tenants.Tenant)));
    }

    [Fact] void should_not_fail() => _exception.ShouldBeNull();
}
#endif
