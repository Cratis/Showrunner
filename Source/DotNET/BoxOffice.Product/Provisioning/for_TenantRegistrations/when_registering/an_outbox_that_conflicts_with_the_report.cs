// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Product.Provisioning.for_TenantRegistrations.given;

namespace Cratis.Showrunner.BoxOffice.Product.Provisioning.for_TenantRegistrations.when_registering;

public class an_outbox_that_conflicts_with_the_report : a_tenant_registration_with_answered_appends
{
    Exception _exception;

    async Task Because()
    {
        _exception = await Catch.Exception(() => RegistrationsWhoseOutboxAnswers(AppendAnswers.Conflicting).Registered(Registered, _context));
        await Read();
    }

    [Fact] void should_leave_the_earlier_answer_standing() => _exception.ShouldBeNull();
    [Fact] void should_still_record_the_tenant() => _tenantEvents.Count.ShouldEqual(2);
}
#endif
