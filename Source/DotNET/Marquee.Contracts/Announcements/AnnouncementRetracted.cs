// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Marquee.Contracts.Announcements;

/// <summary>
/// Published when an announcement is withdrawn before its window ends. A retracted announcement is no longer shown to anyone.
/// </summary>
/// <param name="AnnouncementId">The announcement.</param>
/// <param name="Product">The product the announcement is for.</param>
[EventType]
[Public]
public record AnnouncementRetracted(AnnouncementId AnnouncementId, Product Product);
