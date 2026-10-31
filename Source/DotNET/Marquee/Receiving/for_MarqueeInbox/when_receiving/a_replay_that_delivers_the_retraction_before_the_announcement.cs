// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.given;
using Cratis.Showrunner.Marquee.Receiving.for_MarqueeInbox.given;

namespace Cratis.Showrunner.Marquee.Receiving.for_MarqueeInbox.when_receiving;

/// <summary>
/// An out-of-order replay never resurrects a retracted announcement: what arrives after the retraction is dropped.
/// </summary>
public class a_replay_that_delivers_the_retraction_before_the_announcement : a_marquee_inbox
{
    async Task Because()
    {
        await Delivered(Retracted());
        await Delivered(Published());
        await Delivered(Revised());
        await Delivered(AudienceChanged());
        await ReadRecorded();
    }

    [Fact] void should_record_only_the_retraction() => _recorded.Select(appended => appended.Content.GetType()).ShouldEqual([typeof(AnnouncementRetractionReceived)]);
}
#endif
