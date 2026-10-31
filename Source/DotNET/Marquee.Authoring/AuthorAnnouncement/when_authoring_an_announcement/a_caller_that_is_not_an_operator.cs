// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.Authoring.given;

namespace Cratis.Showrunner.Marquee.Authoring.AuthorAnnouncement.when_authoring_an_announcement;

/// <summary>
/// A caller the host's authoring policy does not admit may not author.
/// </summary>
public class a_caller_that_is_not_an_operator : an_announcement_command<AuthorAnnouncement>
{
    protected override System.Security.Claims.ClaimsPrincipal Principal => Operators.Somebody;

    async Task Because() => _result = await _scenario.Execute(new AuthorAnnouncement(_announcementId, AnnouncementEvents.Product, Audience.Everyone(), "Scheduled maintenance", "Ledger is read-only for two hours.", Display.Banner, Importance.Warning, AnnouncementEvents.MaintenanceStarts, AnnouncementEvents.MaintenanceEnds));

    [Fact] void should_not_be_authorized() => _result.IsAuthorized.ShouldBeFalse();

    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
