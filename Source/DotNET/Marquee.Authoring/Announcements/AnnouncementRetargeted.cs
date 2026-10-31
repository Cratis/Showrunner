// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Marquee.Authoring.Announcements;

/// <summary>
/// An announcement was given another audience. Published to the product as <see cref="AnnouncementAudienceChanged"/>.
/// </summary>
/// <param name="Audience">Who it is for now.</param>
[EventType]
public record AnnouncementRetargeted(Audience Audience);
