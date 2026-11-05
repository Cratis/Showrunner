// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Product.Activity.for_ActivityReporter.given;

namespace Cratis.Showrunner.BoxOffice.Product.Activity.for_ActivityReporter.when_reporting;

public class two_people_on_a_day : an_activity_reporter
{
    async Task Because()
    {
        await OrganizationIsKnown();
        await _reporter.Report(Tenants.Tenant, Tenants.Ada);
        await _reporter.Report(Tenants.Tenant, Tenants.Bo);
        await ReadOutbox();
    }

    [Fact] void should_tell_the_operator_about_each() => _outbox.Count.ShouldEqual(2);
}
#endif
