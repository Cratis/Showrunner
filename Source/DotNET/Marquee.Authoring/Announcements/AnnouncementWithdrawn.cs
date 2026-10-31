// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Marquee.Authoring.Announcements;

/// <summary>
/// An announcement was withdrawn: it is no longer shown to anyone, and nothing can be done to it. Published to the
/// product as <see cref="AnnouncementRetracted"/>.
/// </summary>
[EventType]
public record AnnouncementWithdrawn();
