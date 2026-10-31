// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.BoxOffice.Offering.Metering.for_ComputeDayExtensions;

public class when_finding_when_a_day_closes : Specification
{
    DateTimeOffset _closes;
    ComputeDay _beforeClose;
    ComputeDay _atClose;

    void Because()
    {
        _closes = new ComputeDay(new DateOnly(2026, 10, 30)).ClosesAt(TimeSpan.FromHours(6));
        _beforeClose = ComputeDayExtensions.LatestClosedAt(_closes.AddTicks(-1), TimeSpan.FromHours(6));
        _atClose = ComputeDayExtensions.LatestClosedAt(_closes, TimeSpan.FromHours(6));
    }

    [Fact] void should_close_six_hours_after_midnight_at_the_end_of_the_day() => _closes.ShouldEqual(new DateTimeOffset(2026, 10, 31, 6, 0, 0, TimeSpan.Zero));
    [Fact] void should_not_have_closed_the_day_a_moment_before() => _beforeClose.Value.ShouldEqual(new DateOnly(2026, 10, 29));
    [Fact] void should_have_closed_the_day_at_that_moment() => _atClose.Value.ShouldEqual(new DateOnly(2026, 10, 30));
}
#endif
