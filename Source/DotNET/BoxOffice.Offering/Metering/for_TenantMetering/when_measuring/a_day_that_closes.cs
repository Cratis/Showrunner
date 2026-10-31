// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Metering.for_TenantMetering.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Metering.for_TenantMetering.when_measuring;

public class a_day_that_closes : a_tenant_being_measured
{
    async Task Because()
    {
        await SessionEnded(_noon, 30);
        await SessionEnded(_noon.AddHours(2), 45);
        await StartTheProduct().Measure(_eventStore, _tenant, new(2026, 10, 31, 6, 0, 0, TimeSpan.Zero));
        await ReadReported();
    }

    [Fact] void should_report_one_closed_day() => Reported<ComputeDayClosed>().Count().ShouldEqual(1);
    [Fact] void should_report_it_for_the_tenant() => Reported<ComputeDayClosed>().Single().TenantId.ShouldEqual(_tenant);
    [Fact] void should_report_the_day() => Reported<ComputeDayClosed>().Single().Day.Value.ShouldEqual(new DateOnly(2026, 10, 30));
    [Fact] void should_report_the_total_of_the_day() => Reported<ComputeDayClosed>().Single().Usage.Single().Quantity.Value.ShouldEqual(75L);
    [Fact] void should_report_the_total_of_the_meter() => Reported<ComputeDayClosed>().Single().Usage.Single().Meter.ShouldEqual(new MeterId("compute"));
    [Fact] void should_report_under_the_organization() => _reported.Single().Context.EventSourceId.Value.ShouldEqual(_organization.Value.ToString());
}
#endif
