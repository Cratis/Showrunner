// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Seats;

/// <summary>
/// A number of seats.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record SeatCount(int Value) : ConceptAs<int>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="SeatCount"/>.
    /// </summary>
    public static readonly SeatCount NotSet = new(0);

    /// <summary>
    /// Implicitly converts a <see cref="int"/> to a <see cref="SeatCount"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator SeatCount(int value) => new(value);
}
