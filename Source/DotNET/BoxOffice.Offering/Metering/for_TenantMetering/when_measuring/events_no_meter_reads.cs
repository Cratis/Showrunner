// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Metering.for_TenantMetering.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Metering.for_TenantMetering.when_measuring;

public class events_no_meter_reads : a_tenant_being_measured
{
    async Task Because()
    {
        await _eventStore.EventLog.Append(Guid.NewGuid().ToString(), new SomethingElseHappened());
        await StartTheProduct().Measure(_eventStore, _tenant, new(2026, 10, 31, 6, 0, 0, TimeSpan.Zero));
        await ReadReported();
    }

    [Fact] void should_report_nothing() => _reported.Count.ShouldEqual(0);
}
#endif
