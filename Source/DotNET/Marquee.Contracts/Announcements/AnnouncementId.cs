// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Marquee.Contracts.Announcements;

/// <summary>
/// Identifies one announcement. Announcements of a product share one product-wide stream, so the id is a payload field
/// on every announcement event, not the event source id. It is a <see cref="ConceptAs{T}"/> over a GUID.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record AnnouncementId(Guid Value) : ConceptAs<Guid>(Value), IGeneratable<AnnouncementId>
{
    /// <summary>
    /// Implicitly converts a <see cref="Guid"/> to an <see cref="AnnouncementId"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator AnnouncementId(Guid value) => new(value);

    /// <summary>
    /// Creates a new <see cref="AnnouncementId"/>.
    /// </summary>
    /// <returns>A new <see cref="AnnouncementId"/>.</returns>
    public static AnnouncementId New() => new(GenerateValue.Uuid());
}
