// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Product.Tenancy;
using Cratis.Showrunner.BoxOffice.Seats;

namespace Cratis.Showrunner.BoxOffice.Product.Licensing.for_SeatLicenses.when_asking;

public class the_tenant_is_not_known : Specification
{
    SeatLicense _license;

    async Task Because()
    {
        var stores = Substitute.For<ITenantStores>();
        stores.For(Arg.Any<TenantId>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult<Cratis.Chronicle.IEventStore?>(null));
        _license = await new SeatLicenses(stores, new Clock(Tenants.Now)).For(Tenants.Tenant, SeatHolder.Of(Tenants.Ada));
    }

    [Fact] void should_say_the_organization_is_unknown() => _license.Status.ShouldEqual(SeatLicenseStatus.UnknownOrganization);
}
#endif
