// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.BoxOffice.given;

/// <summary>
/// A <see cref="TimeProvider"/> that stands at a moment, which a specification can move.
/// </summary>
/// <param name="now">Where the clock starts.</param>
public class Clock(DateTimeOffset now) : TimeProvider
{
    DateTimeOffset _now = now;

    /// <summary>
    /// Moves the clock to a day, at noon.
    /// </summary>
    /// <param name="day">The day.</param>
    public void MoveTo(DateOnly day) => _now = new(day.ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero);

    /// <summary>
    /// Moves the clock forward.
    /// </summary>
    /// <param name="time">How far.</param>
    public void Advance(TimeSpan time) => _now += time;

    /// <inheritdoc/>
    public override DateTimeOffset GetUtcNow() => _now;
}
#endif
