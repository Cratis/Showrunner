// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.given;
using Cratis.Showrunner.Marquee.Receiving.for_MarqueeInbox.given;

namespace Cratis.Showrunner.Marquee.Receiving.for_MarqueeInbox.when_receiving;

/// <summary>
/// A repeated delivery changes nothing: the second copy of the same event yields no second fact.
/// </summary>
public class the_same_event_delivered_twice : a_marquee_inbox
{
    async Task Because()
    {
        var context = NextContext();
        await Delivered(Published(), context);
        await Delivered(Published(), context);
        await ReadRecorded();
    }

    [Fact] void should_record_one_fact() => _recorded.Count.ShouldEqual(1);
}
#endif
