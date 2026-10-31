// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Marquee.Contracts.Announcements;

/// <summary>
/// The exact UTC time an announcement stops being shown. An announcement with no end uses <see cref="Indefinitely"/>,
/// so the payload never carries a null.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record ActiveUntil(DateTimeOffset Value) : ConceptAs<DateTimeOffset>(Value)
{
    /// <summary>
    /// Represents an announcement that stays active until it is retracted.
    /// </summary>
    public static readonly ActiveUntil Indefinitely = new(DateTimeOffset.MaxValue);

    /// <summary>
    /// Implicitly converts a <see cref="DateTimeOffset"/> to an <see cref="ActiveUntil"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator ActiveUntil(DateTimeOffset value) => new(value);
}
