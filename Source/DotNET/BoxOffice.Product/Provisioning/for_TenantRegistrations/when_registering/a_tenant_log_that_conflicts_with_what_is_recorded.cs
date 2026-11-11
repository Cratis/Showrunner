// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Product.Provisioning.for_TenantRegistrations.given;

namespace Cratis.Showrunner.BoxOffice.Product.Provisioning.for_TenantRegistrations.when_registering;

public class a_tenant_log_that_conflicts_with_what_is_recorded : a_tenant_registration_with_answered_appends
{
    Exception _exception;

    async Task Because()
    {
        _exception = await Catch.Exception(() => RegistrationsWhoseTenantLogAnswers(AppendAnswers.Conflicting).Registered(Registered, _context));
        await Read();
    }

    [Fact] void should_accept_that_it_was_recorded_before() => _exception.ShouldBeNull();
    [Fact] void should_report_the_tenant_ready() => _outbox.Single().Content.ShouldEqual(new TenantProvisioned(Tenants.Tenant));
}
#endif
