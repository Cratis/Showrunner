// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Marquee.Contracts.Announcements;

/// <summary>
/// Published when the wording or the window of a published announcement changes. The audience and the way it is
/// displayed are not part of a revision; <see cref="AnnouncementAudienceChanged"/> covers the audience. The event states
/// the full new text and window, so a reader never has to combine it with earlier events.
/// </summary>
/// <param name="AnnouncementId">The announcement.</param>
/// <param name="Product">The product the announcement is for.</param>
/// <param name="Title">The headline.</param>
/// <param name="Body">The text.</param>
/// <param name="Importance">How much attention it asks for.</param>
/// <param name="ActiveFrom">When it starts to be shown.</param>
/// <param name="ActiveUntil">When it stops being shown.</param>
[EventType]
[Public]
public record AnnouncementRevised(AnnouncementId AnnouncementId, Product Product, AnnouncementTitle Title, AnnouncementBody Body, Importance Importance, ActiveFrom ActiveFrom, ActiveUntil ActiveUntil);
