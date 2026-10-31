// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.given;
using Cratis.Showrunner.Marquee.Receiving.for_MarqueeInbox.given;

namespace Cratis.Showrunner.Marquee.Receiving.for_MarqueeInbox.when_receiving;

/// <summary>
/// What follows the announcement is recorded in order.
/// </summary>
public class a_revision_and_a_new_audience : a_marquee_inbox
{
    async Task Because()
    {
        await Delivered(Published());
        await Delivered(Revised());
        await Delivered(AudienceChanged());
        await ReadRecorded();
    }

    [Fact]
    void should_record_each_in_order() =>
        _recorded.Select(appended => appended.Content.GetType()).ShouldEqual([typeof(AnnouncementReceived), typeof(AnnouncementRevisionReceived), typeof(AnnouncementAudienceChangeReceived)]);

    [Fact] void should_record_them_all_under_the_announcement_id() => _recorded.All(appended => appended.Context.EventSourceId == _announcementId).ShouldBeTrue();
}
#endif
