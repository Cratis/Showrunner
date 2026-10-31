// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.Authoring.given;

namespace Cratis.Showrunner.Marquee.Authoring.AuthorAnnouncement.when_authoring_an_announcement;

/// <summary>
/// Authoring an id that is already authored is refused, so a retried command cannot author a second one.
/// </summary>
public class an_announcement_that_is_already_authored : an_announcement_command<AuthorAnnouncement>
{
    void Establish() => Given(AnnouncementEvents.Maintenance);

    async Task Because() => _result = await _scenario.Execute(new AuthorAnnouncement(_announcementId, AnnouncementEvents.Product, Audience.Everyone(), "Scheduled maintenance", "Ledger is read-only for two hours.", Display.Banner, Importance.Warning, AnnouncementEvents.MaintenanceStarts, AnnouncementEvents.MaintenanceEnds));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();

    [Fact] void should_have_validation_errors() => _result.ShouldHaveValidationErrors();

    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
