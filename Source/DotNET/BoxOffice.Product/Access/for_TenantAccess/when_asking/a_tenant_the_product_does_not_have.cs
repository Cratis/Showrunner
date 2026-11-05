// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Product.Tenancy;

namespace Cratis.Showrunner.BoxOffice.Product.Access.for_TenantAccess.when_asking;

public class a_tenant_the_product_does_not_have : Specification
{
    bool _readOnly;

    async Task Because()
    {
        var stores = Substitute.For<ITenantStores>();
        stores.For(Arg.Any<TenantId>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult<Cratis.Chronicle.IEventStore?>(null));
        _readOnly = await new TenantAccess(stores, new Clock(Tenants.Now)).IsReadOnly(Tenants.Tenant);
    }

    [Fact] void should_be_read_only() => _readOnly.ShouldBeTrue();
}
#endif
