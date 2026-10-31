// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Marquee.Contracts.Audiences;

namespace Cratis.Showrunner.Marquee.Contracts.Announcements;

/// <summary>
/// Published when a published announcement is retargeted at a different audience.
/// </summary>
/// <param name="AnnouncementId">The announcement.</param>
/// <param name="Product">The product the announcement is for.</param>
/// <param name="Audience">Who the announcement reaches from now on.</param>
[EventType]
[Public]
public record AnnouncementAudienceChanged(AnnouncementId AnnouncementId, Product Product, Audience Audience);
