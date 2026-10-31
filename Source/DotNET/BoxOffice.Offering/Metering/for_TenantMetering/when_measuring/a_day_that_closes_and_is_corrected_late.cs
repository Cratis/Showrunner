// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Diagnostics.Metrics;
using Cratis.Showrunner.BoxOffice.Offering.Metering.for_TenantMetering.given;

namespace Cratis.Showrunner.BoxOffice.Offering.Metering.for_TenantMetering.when_measuring;

public class a_day_that_closes_and_is_corrected_late : a_tenant_being_measured
{
    readonly Dictionary<string, long> _measurements = [];

    async Task Because()
    {
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, subscribed) =>
        {
            if (ReferenceEquals(instrument.Meter, _instruments))
            {
                subscribed.EnableMeasurementEvents(instrument);
            }
        };
        listener.SetMeasurementEventCallback<int>((instrument, value, _, _) =>
        {
            lock (_measurements)
            {
                _measurements[instrument.Name] = _measurements.GetValueOrDefault(instrument.Name) + value;
            }
        });
        listener.Start();

        await SessionEnded(_noon, 30);
        await SessionEnded(_noon.AddHours(1), 30);
        var closes = new DateTimeOffset(2026, 10, 31, 6, 0, 0, TimeSpan.Zero);
        await StartTheProduct().Measure(_eventStore, _tenant, closes);
        await SessionEnded(_noon.AddHours(10), 20);
        await StartTheProduct().Measure(_eventStore, _tenant, closes.AddMinutes(5));
    }

    [Fact] void should_count_every_bucket_update() => _measurements["boxoffice.metering.buckets"].ShouldEqual(3L);
    [Fact] void should_count_the_closed_day() => _measurements["boxoffice.metering.days.closed"].ShouldEqual(1L);
    [Fact] void should_count_the_correction() => _measurements["boxoffice.metering.corrections"].ShouldEqual(1L);
}
#endif
