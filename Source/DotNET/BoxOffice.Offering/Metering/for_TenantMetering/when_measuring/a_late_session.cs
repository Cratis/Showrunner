// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Metering.for_TenantMetering.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Metering.for_TenantMetering.when_measuring;

public class a_late_session : a_tenant_being_measured
{
    async Task Because()
    {
        await SessionEnded(_noon, 30);
        var closes = new DateTimeOffset(2026, 10, 31, 6, 0, 0, TimeSpan.Zero);
        await StartTheProduct().Measure(_eventStore, _tenant, closes);

        // A session of the closed day ends after the day closed.
        await SessionEnded(_noon.AddHours(10), 20);
        await StartTheProduct().Measure(_eventStore, _tenant, closes.AddMinutes(5));
        await StartTheProduct().Measure(_eventStore, _tenant, closes.AddMinutes(10));
        await ReadReported();
    }

    [Fact] void should_report_the_day_closed_once() => Reported<ComputeDayClosed>().Count().ShouldEqual(1);
    [Fact] void should_report_one_correction() => Reported<ComputeDayCorrected>().Count().ShouldEqual(1);
    [Fact] void should_correct_the_same_day() => Reported<ComputeDayCorrected>().Single().Day.Value.ShouldEqual(new DateOnly(2026, 10, 30));
    [Fact] void should_carry_the_corrected_total_and_not_the_difference() => Reported<ComputeDayCorrected>().Single().Usage.Single().Quantity.Value.ShouldEqual(50L);
}
#endif
