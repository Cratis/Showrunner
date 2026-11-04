// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Product.given;
using Cratis.Showrunner.BoxOffice.Seats;

namespace Cratis.Showrunner.BoxOffice.Product.Licensing.for_SeatLicenses.when_asking;

public class the_pool_has_shrunk_below_what_is_held : a_tenant_in_the_product
{
    SeatLicense _first;
    SeatLicense _last;

    async Task Because()
    {
        SeatsGranted(1);
        Holding(Tenants.Ada, Tenants.Bo);
        var licenses = new SeatLicenses(_stores, _clock);
        _first = await licenses.For(Tenants.Tenant, SeatHolder.Of(Tenants.Ada));
        _last = await licenses.For(Tenants.Tenant, SeatHolder.Of(Tenants.Bo));
    }

    [Fact] void should_keep_the_person_who_held_a_seat_first_licensed() => _first.Status.ShouldEqual(SeatLicenseStatus.Licensed);
    [Fact] void should_not_license_the_person_who_is_not_covered() => _last.Status.ShouldEqual(SeatLicenseStatus.NoLicense);
}
#endif
