// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Product.Activity.for_ActivityReporter.given;

namespace Cratis.Showrunner.BoxOffice.Product.Activity.for_ActivityReporter.when_reporting;

public class a_person_who_is_active : an_activity_reporter
{
    ActivityOutcome _outcome;

    async Task Because()
    {
        await OrganizationIsKnown();
        _outcome = await _reporter.Report(Tenants.Tenant, Tenants.Ada);
        await ReadOutbox();
    }

    [Fact] void should_report_the_person() => _outcome.ShouldEqual(ActivityOutcome.Reported);
    [Fact] void should_tell_the_operator_who_was_active_on_the_day() => _outbox.Single().Content.ShouldEqual(new SeatHolderActive(Tenants.Tenant, Tenants.Ada, Tenants.Today));
    [Fact] void should_tell_it_under_the_operators_organization() => _outbox.Single().Context.EventSourceId.Value.ShouldEqual(Tenants.Organization.Value.ToString("D"));
}
#endif
