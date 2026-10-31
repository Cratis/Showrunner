// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.given;
using Cratis.Showrunner.Marquee.Receiving.for_MarqueeInbox.given;

namespace Cratis.Showrunner.Marquee.Receiving.for_MarqueeInbox.when_receiving;

/// <summary>
/// A retraction is recorded.
/// </summary>
public class a_retraction : a_marquee_inbox
{
    async Task Because()
    {
        await Delivered(Published());
        await Delivered(Retracted());
        await ReadRecorded();
    }

    [Fact] void should_record_that_it_was_retracted() => _recorded[^1].Content.ShouldBeOfExactType<AnnouncementRetractionReceived>();
}
#endif
