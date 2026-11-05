// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Provisioning.for_TenantRegistrations.given;

namespace Cratis.Showrunner.BoxOffice.Product.Provisioning.for_TenantRegistrations.when_registering;

public class provisioning_that_fails : a_tenant_registration
{
    Exception _exception;

    async Task Because()
    {
        _provisioner.Provision(Arg.Any<TenantToProvision>(), Arg.Any<CancellationToken>()).Returns<TenantProvisioning>(_ => throw new InvalidOperationException("The database is not reachable"));
        _exception = await Catch.Exception(() => _registrations.Registered(Registered, _context));
        await Read();
    }

    [Fact] void should_fail_so_the_registration_is_retried() => _exception.ShouldBeOfExactType<InvalidOperationException>();
    [Fact] void should_report_nothing() => _outbox.ShouldBeEmpty();
}
#endif
