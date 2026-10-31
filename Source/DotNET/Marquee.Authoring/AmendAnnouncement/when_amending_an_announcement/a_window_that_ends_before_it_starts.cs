// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.Authoring.given;

namespace Cratis.Showrunner.Marquee.Authoring.AmendAnnouncement.when_amending_an_announcement;

/// <summary>
/// A window that ends before it starts is refused.
/// </summary>
public class a_window_that_ends_before_it_starts : an_announcement_command<AmendAnnouncement>
{
    void Establish() => Given(AnnouncementEvents.Maintenance);

    async Task Because() => _result = await _scenario.Execute(new AmendAnnouncement(_announcementId, "Title", "Body", Importance.Warning, AnnouncementEvents.MaintenanceEnds, AnnouncementEvents.MaintenanceStarts));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();

    [Fact] void should_have_validation_errors() => _result.ShouldHaveValidationErrors();

    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
