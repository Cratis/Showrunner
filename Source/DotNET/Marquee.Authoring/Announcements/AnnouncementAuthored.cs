// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Marquee.Authoring.Announcements;

/// <summary>
/// An announcement was authored for a product: who it is for, what it says, how it is shown and when. Appended under
/// the announcement's id; published to the product as <see cref="AnnouncementPublished"/>.
/// </summary>
/// <param name="Product">The product the announcement is for.</param>
/// <param name="Audience">Who it is for.</param>
/// <param name="Title">Its title.</param>
/// <param name="Body">What it says.</param>
/// <param name="Display">How it is shown; it never changes.</param>
/// <param name="Importance">How important it is.</param>
/// <param name="ActiveFrom">When it starts to be shown; <see cref="ActiveFrom.Immediately"/> when it has no start.</param>
/// <param name="ActiveUntil">When it stops being shown; <see cref="ActiveUntil.Indefinitely"/> when it has no end.</param>
[EventType]
public record AnnouncementAuthored(
    Product Product,
    Audience Audience,
    AnnouncementTitle Title,
    AnnouncementBody Body,
    Display Display,
    Importance Importance,
    ActiveFrom ActiveFrom,
    ActiveUntil ActiveUntil);
