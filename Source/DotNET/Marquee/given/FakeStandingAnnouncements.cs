// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Reactive.Subjects;

namespace Cratis.Showrunner.Marquee.given;

/// <summary>
/// An <see cref="IStandingAnnouncements"/> holding the announcements a specification gives it, as the product-wide
/// namespace would, whichever tenant asks.
/// </summary>
/// <param name="held">The announcements.</param>
public class FakeStandingAnnouncements(params StandingAnnouncement[] held) : IStandingAnnouncements
{
    readonly BehaviorSubject<IEnumerable<StandingAnnouncement>> _subject = new(held);

    /// <inheritdoc/>
    public ISubject<IEnumerable<StandingAnnouncement>> Observe() => _subject;

    /// <inheritdoc/>
    public Task<StandingAnnouncement?> Find(AnnouncementId announcementId, CancellationToken cancellationToken = default) =>
        Task.FromResult(held.FirstOrDefault(announcement => announcement.Id == announcementId));
}
#endif
