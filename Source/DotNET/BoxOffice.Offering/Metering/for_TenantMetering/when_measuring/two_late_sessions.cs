// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Metering.for_TenantMetering.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Metering.for_TenantMetering.when_measuring;

public class two_late_sessions : a_tenant_being_measured
{
    async Task Because()
    {
        await SessionEnded(_noon, 30);
        var closes = new DateTimeOffset(2026, 10, 31, 6, 0, 0, TimeSpan.Zero);
        await StartTheProduct().Measure(_eventStore, _tenant, closes);

        await SessionEnded(_noon.AddHours(10), 20);
        await StartTheProduct().Measure(_eventStore, _tenant, closes.AddMinutes(5));
        await SessionEnded(_noon.AddHours(11), 5);
        await StartTheProduct().Measure(_eventStore, _tenant, closes.AddMinutes(10));
        await ReadReported();
    }

    [Fact] void should_report_a_correction_for_each_late_session() => Reported<ComputeDayCorrected>().Count().ShouldEqual(2);
    [Fact] void should_end_with_the_total_of_all_of_them() => Reported<ComputeDayCorrected>().Last().Usage.Single().Quantity.Value.ShouldEqual(55L);
}
#endif
