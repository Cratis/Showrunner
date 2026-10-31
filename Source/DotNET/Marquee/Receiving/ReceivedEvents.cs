// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Marquee.Receiving;

/// <summary>
/// The product received an announcement: who it is for, what it says, how it is shown and when. Translated from
/// <c>AnnouncementPublished</c> and appended under the announcement's id to the product-wide stream.
/// </summary>
/// <param name="Audience">Who it is for.</param>
/// <param name="Title">Its title.</param>
/// <param name="Body">What it says.</param>
/// <param name="Display">How it is shown.</param>
/// <param name="Importance">How important it is.</param>
/// <param name="ActiveFrom">When it starts to be shown.</param>
/// <param name="ActiveUntil">When it stops being shown.</param>
[EventType]
public record AnnouncementReceived(
    Audience Audience,
    AnnouncementTitle Title,
    AnnouncementBody Body,
    Display Display,
    Importance Importance,
    ActiveFrom ActiveFrom,
    ActiveUntil ActiveUntil);

/// <summary>
/// The product received a revision of an announcement. Translated from <c>AnnouncementRevised</c>.
/// </summary>
/// <param name="Title">Its title.</param>
/// <param name="Body">What it says.</param>
/// <param name="Importance">How important it is.</param>
/// <param name="ActiveFrom">When it starts to be shown.</param>
/// <param name="ActiveUntil">When it stops being shown.</param>
[EventType]
public record AnnouncementRevisionReceived(
    AnnouncementTitle Title,
    AnnouncementBody Body,
    Importance Importance,
    ActiveFrom ActiveFrom,
    ActiveUntil ActiveUntil);

/// <summary>
/// The product received another audience for an announcement. Translated from <c>AnnouncementAudienceChanged</c>.
/// </summary>
/// <param name="Audience">Who it is for now.</param>
[EventType]
public record AnnouncementAudienceChangeReceived(Audience Audience);

/// <summary>
/// The product received the retraction of an announcement: it is shown to no one, and nothing received about it
/// afterwards brings it back, also when the retraction arrives before the announcement does. Translated from
/// <c>AnnouncementRetracted</c>.
/// </summary>
[EventType]
public record AnnouncementRetractionReceived();
