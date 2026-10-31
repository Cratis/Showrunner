// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Marquee.Authoring.Announcements;

/// <summary>
/// What an announcement says, how important it is or when it is shown was amended; how it is displayed and who it is
/// for are not part of it. Published to the product as <see cref="AnnouncementRevised"/>.
/// </summary>
/// <param name="Title">Its title.</param>
/// <param name="Body">What it says.</param>
/// <param name="Importance">How important it is.</param>
/// <param name="ActiveFrom">When it starts to be shown.</param>
/// <param name="ActiveUntil">When it stops being shown.</param>
[EventType]
public record AnnouncementAmended(
    AnnouncementTitle Title,
    AnnouncementBody Body,
    Importance Importance,
    ActiveFrom ActiveFrom,
    ActiveUntil ActiveUntil);
