// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Product.Activity.for_ActivityReporter.given;

namespace Cratis.Showrunner.BoxOffice.Product.Activity.for_ActivityReporter.when_reporting;

public class the_same_person_after_a_restart : an_activity_reporter
{
    ActivityOutcome _outcome;

    async Task Because()
    {
        await OrganizationIsKnown();
        await _reporter.Report(Tenants.Tenant, Tenants.Ada);

        // A new reporter has remembered nobody: the event log's key still keeps the day's repeat out.
        _outcome = await Reporter().Report(Tenants.Tenant, Tenants.Ada);
        await ReadOutbox();
    }

    [Fact] void should_know_the_person_was_reported() => _outcome.ShouldEqual(ActivityOutcome.AlreadyReported);
    [Fact] void should_tell_the_operator_once() => _outbox.Count.ShouldEqual(1);
}
#endif
