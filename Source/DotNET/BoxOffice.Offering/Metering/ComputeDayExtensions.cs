// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Offering.Metering;

/// <summary>
/// Extension methods for <see cref="ComputeDay"/>.
/// </summary>
public static class ComputeDayExtensions
{
    /// <summary>
    /// Gets when a day closes: midnight at its end, plus the delay.
    /// </summary>
    /// <param name="day">The UTC day.</param>
    /// <param name="delay">How long after the day ends it closes.</param>
    /// <returns>The moment the day closes.</returns>
    public static DateTimeOffset ClosesAt(this ComputeDay day, TimeSpan delay) =>
        new DateTimeOffset(day.Value.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero) + delay;

    /// <summary>
    /// Gets the latest day that is closed at a moment.
    /// </summary>
    /// <param name="now">The moment.</param>
    /// <param name="delay">How long after a day ends it closes.</param>
    /// <returns>The latest closed day.</returns>
    public static ComputeDay LatestClosedAt(DateTimeOffset now, TimeSpan delay) =>
        new(DateOnly.FromDateTime((now.ToUniversalTime() - delay).Date).AddDays(-1));
}
