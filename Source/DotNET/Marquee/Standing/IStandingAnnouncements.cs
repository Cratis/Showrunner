// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Subjects;

namespace Cratis.Showrunner.Marquee.Standing;

/// <summary>
/// Defines access to the announcements the product holds, which live in the product's own namespace and are read from
/// the namespace of any tenant.
/// </summary>
public interface IStandingAnnouncements
{
    /// <summary>
    /// Observes every announcement the product holds, again every time one changes.
    /// </summary>
    /// <returns>The announcements.</returns>
    ISubject<IEnumerable<StandingAnnouncement>> Observe();

    /// <summary>
    /// Finds an announcement the product holds.
    /// </summary>
    /// <param name="announcementId">The announcement.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The announcement, or <see langword="null"/> when the product holds none with that id.</returns>
    Task<StandingAnnouncement?> Find(AnnouncementId announcementId, CancellationToken cancellationToken = default);
}
