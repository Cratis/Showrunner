// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Provisioning.for_TenantRegistrations.given;

namespace Cratis.Showrunner.BoxOffice.Product.Provisioning.for_TenantRegistrations.when_registering;

public class the_same_registration_twice : a_tenant_registration
{
    async Task Because()
    {
        // The registration is delivered again, as when the reactor's partition is recovered.
        await _registrations.Registered(Registered, _context);
        await _registrations.Registered(Registered, _context);
        await Read();
    }

    [Fact] void should_report_the_tenant_ready_once() => _outbox.Count.ShouldEqual(1);
    [Fact] void should_record_in_the_tenant_once() => _tenantEvents.Count.ShouldEqual(2);
}
#endif
