// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.Authoring.given;

namespace Cratis.Showrunner.Marquee.Authoring.AuthorAnnouncement.when_authoring_an_announcement;

/// <summary>
/// A valid announcement is authored with exactly what was asked for.
/// </summary>
public class a_valid_announcement : an_announcement_command<AuthorAnnouncement>
{
    async Task Because() => _result = await _scenario.Execute(new AuthorAnnouncement(_announcementId, AnnouncementEvents.Product, Audience.Everyone(), "Scheduled maintenance", "Ledger is read-only for two hours.", Display.Banner, Importance.Warning, AnnouncementEvents.MaintenanceStarts, AnnouncementEvents.MaintenanceEnds));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_that_it_was_authored() =>
        await _scenario.ShouldHaveAppendedEvent<AuthorAnnouncement, AnnouncementAuthored>(_announcementId,
            e => e.Product == AnnouncementEvents.Product &&
                 e.Audience.Kind == AudienceKind.Everyone &&
                 e.Title == "Scheduled maintenance" &&
                 e.Body == "Ledger is read-only for two hours." &&
                 e.Display == Display.Banner &&
                 e.Importance == Importance.Warning &&
                 e.ActiveFrom.Value == AnnouncementEvents.MaintenanceStarts &&
                 e.ActiveUntil.Value == AnnouncementEvents.MaintenanceEnds);
}
#endif
