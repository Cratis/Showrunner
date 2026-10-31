// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Metering.for_TenantMetering.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Metering.for_TenantMetering.when_measuring;

public class events_that_are_read_again : a_tenant_being_measured
{
    async Task Because()
    {
        await SessionEnded(_noon, 30);
        await StartTheProduct().Measure(_eventStore, _tenant, _noon);

        // The product stopped before it remembered how far it had read.
        await _store.SetCursor(_tenant, EventSequenceNumber.First);
        await StartTheProduct().Measure(_eventStore, _tenant, new(2026, 10, 31, 6, 0, 0, TimeSpan.Zero));
        await ReadReported();
    }

    [Fact] void should_count_each_event_once() => Reported<ComputeDayClosed>().Single().Usage.Single().Quantity.Value.ShouldEqual(30L);
}
#endif
