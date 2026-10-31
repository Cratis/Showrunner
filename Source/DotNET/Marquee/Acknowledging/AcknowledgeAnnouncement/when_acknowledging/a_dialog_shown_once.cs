// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.given;

namespace Cratis.Showrunner.Marquee.Acknowledging.AcknowledgeAnnouncement.when_acknowledging;

/// <summary>
/// The first acknowledgement of a dialog shown once is recorded, in the tenant, for the person.
/// </summary>
public class a_dialog_shown_once : an_acknowledging_person
{
    protected override IEnumerable<StandingAnnouncement> Held => [ProductAnnouncements.DialogOnce(_announcementId)];

    async Task Because() => _result = await _scenario.Execute(new AcknowledgeAnnouncement(_announcementId));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_that_the_person_acknowledged_it() =>
        await _scenario.ShouldHaveAppendedEvent<AcknowledgeAnnouncement, AnnouncementAcknowledged>(
            _announcementId,
            e => e.AnnouncementId == _announcementId && e.UserSubject == Viewers.Me);
}
#endif
