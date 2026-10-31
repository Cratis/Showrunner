// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Marquee.Acknowledging;

/// <summary>
/// A person saw an announcement shown in a dialog once, and will not be shown it again. Appended in the tenant's
/// namespace under the announcement's id, with the person as compliance subject.
/// </summary>
/// <param name="AnnouncementId">The announcement.</param>
/// <param name="UserSubject">Who saw it.</param>
[EventType]
public record AnnouncementAcknowledged(AnnouncementId AnnouncementId, UserSubject UserSubject);
