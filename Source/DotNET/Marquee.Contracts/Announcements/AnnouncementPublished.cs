// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Marquee.Contracts.Audiences;

namespace Cratis.Showrunner.Marquee.Contracts.Announcements;

/// <summary>
/// Published when an announcement is first made. It carries everything a product needs to show the announcement: who it
/// is for, what it says, how it is presented and the exact window it is active in.
/// </summary>
/// <param name="AnnouncementId">The announcement.</param>
/// <param name="Product">The product the announcement is for.</param>
/// <param name="Audience">Who the announcement reaches.</param>
/// <param name="Title">The headline.</param>
/// <param name="Body">The text.</param>
/// <param name="Display">How it is presented.</param>
/// <param name="Importance">How much attention it asks for.</param>
/// <param name="ActiveFrom">When it starts to be shown.</param>
/// <param name="ActiveUntil">When it stops being shown.</param>
[EventType]
[Public]
public record AnnouncementPublished(AnnouncementId AnnouncementId, Product Product, Audience Audience, AnnouncementTitle Title, AnnouncementBody Body, Display Display, Importance Importance, ActiveFrom ActiveFrom, ActiveUntil ActiveUntil);
