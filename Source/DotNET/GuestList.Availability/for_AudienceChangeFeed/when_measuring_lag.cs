// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Diagnostics.Metrics;
using Cratis.Choreography.Availability.Audiences;
using Cratis.Showrunner.GuestList.Availability.given;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cratis.Showrunner.GuestList.Availability.for_AudienceChangeFeed;

/// <summary>
/// The lag is the time since the last change was received. It is not reported before the first one, and it grows while
/// nothing arrives.
/// </summary>
public class when_measuring_lag : Specification
{
    a_clock _clock;
    AudienceChangeFeed _feed;
    GuestListAvailabilityMetrics _metrics;
    TestMeterFactory _factory;
    MeterListener _listener;
    TimeSpan? _beforeAnyChange;
    TimeSpan? _afterAMinute;
    TimeSpan? _afterANewChange;
    double? _reported;
    bool _reportedBefore;

    void Establish()
    {
        _clock = new();
        _feed = new(_clock, NullLogger<AudienceChangeFeed>.Instance);
        _factory = new();
        _metrics = new(_factory, _feed);
        _listener = new()
        {
            InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter == _factory.Created && instrument.Name == GuestListAvailabilityMetrics.LagName)
                {
                    listener.EnableMeasurementEvents(instrument);
                }
            }
        };
        _listener.SetMeasurementEventCallback<double>((_, measurement, _, _) => _reported = measurement);
        _listener.Start();
    }

    async Task Because()
    {
        _beforeAnyChange = _feed.Lag;
        _listener.RecordObservableInstruments();
        _reportedBefore = _reported is not null;
        await _feed.Publish(new(AudienceLevel.System, null));
        _clock.Advance(TimeSpan.FromMinutes(1));
        _afterAMinute = _feed.Lag;
        _listener.RecordObservableInstruments();
        await _feed.Publish(new(AudienceLevel.Tenant, "acme"));
        _afterANewChange = _feed.Lag;
    }

    void Destroy()
    {
        _listener.Dispose();
        _metrics.Dispose();
    }

    [Fact] void should_have_no_lag_before_the_first_change() => _beforeAnyChange.ShouldBeNull();
    [Fact] void should_report_no_measurement_before_the_first_change() => _reportedBefore.ShouldBeFalse();
    [Fact] void should_grow_while_nothing_arrives() => _afterAMinute.ShouldEqual(TimeSpan.FromMinutes(1));
    [Fact] void should_report_it_as_a_metric_in_seconds() => _reported.ShouldEqual(60d);
    [Fact] void should_start_again_when_a_change_arrives() => _afterANewChange.ShouldEqual(TimeSpan.Zero);

    sealed class TestMeterFactory : IMeterFactory
    {
        public Meter Created { get; private set; }

        public Meter Create(MeterOptions options) => Created = new(options);

        public void Dispose()
        {
        }
    }
}
#endif
