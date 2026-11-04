// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Product.Activity.for_ActivityReporter.given;

namespace Cratis.Showrunner.BoxOffice.Product.Activity.for_ActivityReporter.when_reporting;

public class a_tenant_that_does_not_know_its_organization_yet : an_activity_reporter
{
    ActivityOutcome _outcome;
    ActivityOutcome _afterwards;

    async Task Because()
    {
        _outcome = await _reporter.Report(Tenants.Tenant, Tenants.Ada);
        await OrganizationIsKnown();
        _afterwards = await _reporter.Report(Tenants.Tenant, Tenants.Ada);
        await ReadOutbox();
    }

    [Fact] void should_say_the_organization_is_not_known() => _outcome.ShouldEqual(ActivityOutcome.OrganizationNotKnown);
    [Fact] void should_report_the_person_once_it_is_known() => _afterwards.ShouldEqual(ActivityOutcome.Reported);
    [Fact] void should_tell_the_operator_once() => _outbox.Count.ShouldEqual(1);
}
#endif
