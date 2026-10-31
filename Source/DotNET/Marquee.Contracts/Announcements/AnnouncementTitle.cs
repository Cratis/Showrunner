// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Marquee.Contracts.Announcements;

/// <summary>
/// The headline of an announcement.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record AnnouncementTitle(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="AnnouncementTitle"/>.
    /// </summary>
    public static readonly AnnouncementTitle NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="AnnouncementTitle"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator AnnouncementTitle(string value) => new(value);
}
