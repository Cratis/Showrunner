// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Product.Activity.for_ActivityReporter.given;
using Cratis.Showrunner.BoxOffice.Product.Tenancy;

namespace Cratis.Showrunner.BoxOffice.Product.Activity.for_ActivityReporter.when_reporting;

public class a_tenant_the_product_does_not_have : an_activity_reporter
{
    ActivityOutcome _outcome;

    async Task Because()
    {
        _stores = Substitute.For<ITenantStores>();
        _stores.For(Arg.Any<TenantId>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult<Cratis.Chronicle.IEventStore?>(null));
        _outcome = await Reporter().Report(Tenants.Tenant, Tenants.Ada);
        await ReadOutbox();
    }

    [Fact] void should_say_the_tenant_is_unknown() => _outcome.ShouldEqual(ActivityOutcome.UnknownTenant);
    [Fact] void should_tell_the_operator_nothing() => _outbox.ShouldBeEmpty();
}
#endif
