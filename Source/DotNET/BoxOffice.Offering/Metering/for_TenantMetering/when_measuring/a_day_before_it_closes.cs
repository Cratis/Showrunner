// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Metering.for_TenantMetering.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Metering.for_TenantMetering.when_measuring;

public class a_day_before_it_closes : a_tenant_being_measured
{
    async Task Because()
    {
        await SessionEnded(_noon, 30);
        await StartTheProduct().Measure(_eventStore, _tenant, new(2026, 10, 31, 5, 59, 59, TimeSpan.Zero));
        await ReadReported();
    }

    [Fact] void should_not_report_the_day() => _reported.Count.ShouldEqual(0);
    [Fact] async Task should_keep_counting_it() => (await _store.Get(_tenant, new DateOnly(2026, 10, 30)))!.Quantities[new MeterId("compute")].Value.ShouldEqual(30L);
}
#endif
