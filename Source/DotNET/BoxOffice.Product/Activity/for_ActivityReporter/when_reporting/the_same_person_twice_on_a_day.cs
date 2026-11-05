// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Product.Activity.for_ActivityReporter.given;

namespace Cratis.Showrunner.BoxOffice.Product.Activity.for_ActivityReporter.when_reporting;

public class the_same_person_twice_on_a_day : an_activity_reporter
{
    ActivityOutcome _first;
    ActivityOutcome _second;

    async Task Because()
    {
        await OrganizationIsKnown();
        _first = await _reporter.Report(Tenants.Tenant, Tenants.Ada);
        _clock.Advance(TimeSpan.FromHours(3));
        _second = await _reporter.Report(Tenants.Tenant, Tenants.Ada);
        await ReadOutbox();
    }

    [Fact] void should_report_the_first_time() => _first.ShouldEqual(ActivityOutcome.Reported);
    [Fact] void should_not_report_the_second_time() => _second.ShouldEqual(ActivityOutcome.AlreadyReported);
    [Fact] void should_tell_the_operator_once() => _outbox.Count.ShouldEqual(1);
}
#endif
