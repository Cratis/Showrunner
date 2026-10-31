// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.given;
using Cratis.Showrunner.Marquee.Receiving.for_MarqueeInbox.given;

namespace Cratis.Showrunner.Marquee.Receiving.for_MarqueeInbox.when_receiving;

/// <summary>
/// Retracting twice records one retraction.
/// </summary>
public class a_retraction_delivered_twice : a_marquee_inbox
{
    async Task Because()
    {
        await Delivered(Published());
        await Delivered(Retracted());
        await Delivered(Retracted());
        await ReadRecorded();
    }

    [Fact] void should_record_one_retraction() => _recorded.Count(appended => appended.Content is AnnouncementRetractionReceived).ShouldEqual(1);
}
#endif
