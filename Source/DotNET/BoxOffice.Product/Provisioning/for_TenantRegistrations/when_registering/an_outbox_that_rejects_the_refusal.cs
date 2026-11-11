// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Product.Provisioning.for_TenantRegistrations.given;

namespace Cratis.Showrunner.BoxOffice.Product.Provisioning.for_TenantRegistrations.when_registering;

public class an_outbox_that_rejects_the_refusal : a_tenant_registration_with_answered_appends
{
    Exception _exception;

    async Task Because()
    {
        _provisioner.Provision(Arg.Any<TenantToProvision>(), Arg.Any<CancellationToken>()).Returns(TenantProvisioning.Refused("The name is taken"));
        _exception = await Catch.Exception(() => RegistrationsWhoseOutboxAnswers(AppendAnswers.Rejected).Registered(Registered, _context));
    }

    [Fact] void should_say_the_refusal_was_not_recorded() => _exception.ShouldBeOfExactType<TenantNotRecorded>();
}
#endif
