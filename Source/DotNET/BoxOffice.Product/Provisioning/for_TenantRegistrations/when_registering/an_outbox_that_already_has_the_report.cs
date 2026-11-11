// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Product.Provisioning.for_TenantRegistrations.given;

namespace Cratis.Showrunner.BoxOffice.Product.Provisioning.for_TenantRegistrations.when_registering;

public class an_outbox_that_already_has_the_report : a_tenant_registration_with_answered_appends
{
    Exception _exception;

    async Task Because() => _exception = await Catch.Exception(() => RegistrationsWhoseOutboxAnswers(AppendAnswers.AlreadyAppended).Registered(Registered, _context));

    [Fact] void should_accept_that_it_was_reported_before() => _exception.ShouldBeNull();
}
#endif
