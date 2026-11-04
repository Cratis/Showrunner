// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Product.given;
using Cratis.Showrunner.BoxOffice.Seats;

namespace Cratis.Showrunner.BoxOffice.Product.Licensing.for_SeatLicenses.when_asking;

public class the_tenant_is_suspended : a_tenant_in_the_product
{
    SeatLicense _license;

    async Task Because()
    {
        SeatsGranted(5);
        Suspended();
        _license = await new SeatLicenses(_stores, _clock).For(Tenants.Tenant, SeatHolder.Of(Tenants.Ada));
    }

    [Fact] void should_not_license_the_person() => _license.Status.ShouldEqual(SeatLicenseStatus.NoLicense);
}
#endif
