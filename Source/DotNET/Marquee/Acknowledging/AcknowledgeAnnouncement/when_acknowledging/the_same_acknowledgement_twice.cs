// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.given;

namespace Cratis.Showrunner.Marquee.Acknowledging.AcknowledgeAnnouncement.when_acknowledging;

/// <summary>
/// Acknowledging twice appends nothing the second time.
/// </summary>
public class the_same_acknowledgement_twice : an_acknowledging_person
{
    protected override IEnumerable<StandingAnnouncement> Held => [ProductAnnouncements.DialogOnce(_announcementId)];

    void Establish() => Given(new AnnouncementAcknowledged(_announcementId, Viewers.Me));

    async Task Because() => _result = await _scenario.Execute(new AcknowledgeAnnouncement(_announcementId));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
