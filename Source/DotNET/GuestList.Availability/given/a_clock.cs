// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.GuestList.Availability.given;

/// <summary>
/// A <see cref="TimeProvider"/> that only moves when a specification moves it.
/// </summary>
public class a_clock : TimeProvider
{
    DateTimeOffset _now = new(2026, 1, 5, 12, 0, 0, TimeSpan.Zero);

    /// <inheritdoc/>
    public override DateTimeOffset GetUtcNow() => _now;

    /// <summary>
    /// Moves the clock forward.
    /// </summary>
    /// <param name="by">How far.</param>
    public void Advance(TimeSpan by) => _now += by;
}
#endif
