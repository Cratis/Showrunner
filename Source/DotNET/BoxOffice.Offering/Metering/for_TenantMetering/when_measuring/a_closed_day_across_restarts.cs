// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Metering.for_TenantMetering.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Metering.for_TenantMetering.when_measuring;

public class a_closed_day_across_restarts : a_tenant_being_measured
{
    async Task Because()
    {
        await SessionEnded(_noon, 30);
        var closes = new DateTimeOffset(2026, 10, 31, 6, 0, 0, TimeSpan.Zero);
        await StartTheProduct().Measure(_eventStore, _tenant, closes);
        await StartTheProduct().Measure(_eventStore, _tenant, closes.AddMinutes(1));
        await StartTheProduct().Measure(_eventStore, _tenant, closes.AddDays(1));
        await ReadReported();
    }

    [Fact] void should_report_it_once() => Reported<ComputeDayClosed>().Count().ShouldEqual(1);
    [Fact] void should_not_report_a_correction() => Reported<ComputeDayCorrected>().Count().ShouldEqual(0);
}
#endif
