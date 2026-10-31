// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Marquee.Contracts.Announcements;

/// <summary>
/// The text of an announcement, in Markdown.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record AnnouncementBody(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="AnnouncementBody"/>.
    /// </summary>
    public static readonly AnnouncementBody NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="AnnouncementBody"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator AnnouncementBody(string value) => new(value);
}
