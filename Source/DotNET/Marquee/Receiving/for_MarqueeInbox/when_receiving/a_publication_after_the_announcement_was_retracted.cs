// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.given;
using Cratis.Showrunner.Marquee.Receiving.for_MarqueeInbox.given;

namespace Cratis.Showrunner.Marquee.Receiving.for_MarqueeInbox.when_receiving;

/// <summary>
/// A publication that arrives again after the retraction is dropped.
/// </summary>
public class a_publication_after_the_announcement_was_retracted : a_marquee_inbox
{
    async Task Because()
    {
        await Delivered(Published());
        await Delivered(Retracted());
        await Delivered(Published());
        await ReadRecorded();
    }

    [Fact]
    void should_record_the_announcement_and_its_retraction_only() =>
        _recorded.Select(appended => appended.Content.GetType()).ShouldEqual([typeof(AnnouncementReceived), typeof(AnnouncementRetractionReceived)]);
}
#endif
