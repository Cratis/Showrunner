// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Product.given;
using Cratis.Showrunner.BoxOffice.Seats;

namespace Cratis.Showrunner.BoxOffice.Product.Licensing.for_SeatLicenses.when_asking;

public class a_free_seat : a_tenant_in_the_product
{
    SeatLicense _license;

    async Task Because()
    {
        SeatsGranted(3);
        Holding(Tenants.Ada, Tenants.Bo);
        _license = await new SeatLicenses(_stores, _clock).For(Tenants.Tenant, SeatHolder.Of(Tenants.Cy));
    }

    [Fact] void should_license_the_person() => _license.Status.ShouldEqual(SeatLicenseStatus.Licensed);
    [Fact] void should_say_the_person_may_take_a_seat() => _license.MayTakeASeat.ShouldBeTrue();
}
#endif
