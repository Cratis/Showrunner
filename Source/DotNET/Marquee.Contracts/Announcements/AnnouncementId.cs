// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Marquee.Contracts.Announcements;

/// <summary>
/// Identifies one announcement. On the contract events, which every announcement of a product shares one product-wide
/// stream for, the id is a payload field, not the event source id. It is an <see cref="EventSourceId{T}"/> over a GUID
/// because it is the identity of the announcement's own stream where the announcement is authored, received and
/// acknowledged.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record AnnouncementId(Guid Value) : EventSourceId<Guid>(Value), IGeneratable<AnnouncementId>
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
