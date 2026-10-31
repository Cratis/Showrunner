// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.given;
using Cratis.Showrunner.Marquee.Receiving.for_MarqueeInbox.given;

namespace Cratis.Showrunner.Marquee.Receiving.for_MarqueeInbox.when_receiving;

/// <summary>
/// A revision of an announcement the product never received is dropped.
/// </summary>
public class a_revision_of_an_announcement_that_was_never_received : a_marquee_inbox
{
    async Task Because()
    {
        await Delivered(Revised());
        await Delivered(AudienceChanged());
        await ReadRecorded();
    }

    [Fact] void should_record_nothing() => _recorded.ShouldBeEmpty();
}
#endif
