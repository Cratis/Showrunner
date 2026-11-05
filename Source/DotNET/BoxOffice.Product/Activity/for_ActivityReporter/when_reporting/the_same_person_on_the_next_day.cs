// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Product.Activity.for_ActivityReporter.given;

namespace Cratis.Showrunner.BoxOffice.Product.Activity.for_ActivityReporter.when_reporting;

public class the_same_person_on_the_next_day : an_activity_reporter
{
    async Task Because()
    {
        await OrganizationIsKnown();
        await _reporter.Report(Tenants.Tenant, Tenants.Ada);
        _clock.MoveTo(Tenants.Day(1));
        await _reporter.Report(Tenants.Tenant, Tenants.Ada);
        await ReadOutbox();
    }

    [Fact] void should_tell_the_operator_for_each_day() => _outbox.Select(_ => ((SeatHolderActive)_.Content).Day).ShouldContainOnly(new ActivityDay[] { Tenants.Today, Tenants.Day(1) });
}
#endif
