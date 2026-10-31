// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.given;
using Cratis.Showrunner.Marquee.Receiving;

namespace Cratis.Showrunner.Marquee.Standing.for_StandingAnnouncement.when_reducing;

/// <summary>
/// A received announcement is held with everything it arrived with, under its id.
/// </summary>
public class a_received_announcement : a_read_model<StandingAnnouncement>
{
    StandingAnnouncement _announcement;

    async Task Because()
    {
        await Happened(ProductAnnouncements.Received(ProductAnnouncements.DialogOnce(_announcementId)));
        _announcement = _scenario.Instance;
    }

    [Fact] void should_have_the_id_of_the_event_source() => _announcement.Id.ShouldEqual(_announcementId);

    [Fact] void should_have_the_title() => _announcement.Title.ShouldEqual(new AnnouncementTitle("Meet Reporting"));

    [Fact] void should_be_shown_in_a_dialog_once() => _announcement.Display.ShouldEqual(Display.DialogOnce);

    [Fact] void should_have_no_end() => _announcement.ActiveUntil.ShouldEqual(ActiveUntil.Indefinitely);
}
#endif
