// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Showrunner.Marquee.given;

namespace Cratis.Showrunner.Marquee.Acknowledging.AcknowledgeAnnouncement.when_acknowledging;

/// <summary>
/// Nobody who is not signed in can acknowledge an announcement.
/// </summary>
public class a_person_who_is_not_signed_in : an_acknowledging_person
{
    protected override ClaimsPrincipal Principal => Viewers.Anonymous;

    protected override IEnumerable<StandingAnnouncement> Held => [ProductAnnouncements.DialogOnce(_announcementId)];

    async Task Because() => _result = await _scenario.Execute(new AcknowledgeAnnouncement(_announcementId));

    [Fact] void should_not_be_authorized() => _result.IsAuthorized.ShouldBeFalse();

    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
