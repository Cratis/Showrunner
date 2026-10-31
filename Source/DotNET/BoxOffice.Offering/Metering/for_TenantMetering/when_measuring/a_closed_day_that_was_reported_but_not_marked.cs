// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.BoxOffice.Offering.Metering.for_TenantMetering.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Metering.for_TenantMetering.when_measuring;

public class a_closed_day_that_was_reported_but_not_marked : a_tenant_being_measured
{
    async Task Because()
    {
        await SessionEnded(_noon, 30);
        var closes = new DateTimeOffset(2026, 10, 31, 6, 0, 0, TimeSpan.Zero);

        // The product stopped after the event store had the closed day, and before the store marked it reported.
        var outbox = _eventStore.GetEventSequence(EventSequenceId.Outbox);
        await StartTheProduct().Measure(_eventStore, _tenant, closes.AddHours(-1));
        await outbox.AppendOnce(
            _organization,
            new ComputeDayClosed(_tenant, new DateOnly(2026, 10, 30), [new MeterUsage("compute", 30L)]),
            new IdempotencyKey($"boxoffice-compute-closed-{_tenant}-2026-10-30"));

        await StartTheProduct().Measure(_eventStore, _tenant, closes);
        await ReadReported();
        _open = await _store.GetOpen(_tenant, new DateOnly(2026, 10, 30));
    }

    IReadOnlyList<MeteringDay> _open;

    [Fact] void should_still_have_one_closed_day() => Reported<ComputeDayClosed>().Count().ShouldEqual(1);
    [Fact] void should_mark_the_day_reported() => _open.Count.ShouldEqual(0);
}
#endif
