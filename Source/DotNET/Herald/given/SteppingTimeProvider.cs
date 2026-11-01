// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Herald.given;

/// <summary>
/// A clock that stands still until it is moved.
/// </summary>
public class SteppingTimeProvider : TimeProvider
{
    DateTimeOffset _now = new(2026, 11, 1, 12, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// Moves the clock forward.
    /// </summary>
    /// <param name="by">How far.</param>
    public void Advance(TimeSpan by) => _now += by;

    /// <inheritdoc/>
    public override DateTimeOffset GetUtcNow() => _now;
}
#endif
