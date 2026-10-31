// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice;

/// <summary>
/// A value with the time the operator decided it. The newest decision wins, whatever order the decisions arrive in.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
/// <param name="Value">The value.</param>
/// <param name="At">When the operator decided it.</param>
public record Stamped<T>(T Value, DateTimeOffset At)
{
    /// <summary>
    /// Gets the value before any decision was made.
    /// </summary>
    /// <param name="value">The value that stands until the operator decides.</param>
    /// <returns>The stamped value.</returns>
    public static Stamped<T> Undecided(T value) => new(value, DateTimeOffset.MinValue);

    /// <summary>
    /// Takes a decision, unless a newer one was already taken. A decision made at the same time as the standing one
    /// replaces it, so decisions made together apply in the order they arrive.
    /// </summary>
    /// <param name="value">The decided value.</param>
    /// <param name="at">When the operator decided it.</param>
    /// <returns>The value that stands.</returns>
    public Stamped<T> Decide(T value, DateTimeOffset at) => at >= At ? new(value, at) : this;
}
