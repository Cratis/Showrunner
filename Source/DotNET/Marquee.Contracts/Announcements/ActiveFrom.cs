// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Marquee.Contracts.Announcements;

/// <summary>
/// The exact UTC time an announcement starts to be shown. It is a point in time, not a whole day, so a maintenance
/// window can be announced to the minute.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record ActiveFrom(DateTimeOffset Value) : ConceptAs<DateTimeOffset>(Value)
{
    /// <summary>
    /// Represents an announcement that is active from the moment it is published.
    /// </summary>
    public static readonly ActiveFrom Immediately = new(DateTimeOffset.MinValue);

    /// <summary>
    /// Implicitly converts a <see cref="DateTimeOffset"/> to an <see cref="ActiveFrom"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator ActiveFrom(DateTimeOffset value) => new(value);
}
