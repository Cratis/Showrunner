// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Usher.Reporting.Capture.for_CapturedErrors.given;

/// <summary>
/// A clock a specification moves forward by hand.
/// </summary>
/// <param name="now">The time the clock starts at.</param>
public class MovableClock(DateTimeOffset now) : TimeProvider
{
    DateTimeOffset _now = now;

    /// <inheritdoc/>
    public override DateTimeOffset GetUtcNow() => _now;

    /// <summary>
    /// Moves the clock forward.
    /// </summary>
    /// <param name="duration">How far.</param>
    public void Advance(TimeSpan duration) => _now += duration;
}
#endif
