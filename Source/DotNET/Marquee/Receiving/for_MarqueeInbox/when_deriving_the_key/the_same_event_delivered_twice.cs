// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.Marquee.given;
using Cratis.Showrunner.Marquee.Receiving.for_MarqueeInbox.given;

namespace Cratis.Showrunner.Marquee.Receiving.for_MarqueeInbox.when_deriving_the_key;

/// <summary>
/// Every copy of the same event gets the same key, and a different event gets a different one.
/// </summary>
public class the_same_event_delivered_twice : a_marquee_inbox
{
    IdempotencyKey _first;
    IdempotencyKey _second;
    IdempotencyKey _other;

    void Because()
    {
        var context = NextContext();
        _first = MarqueeInbox.KeyFor(Published(), context, AuthoringStore);
        _second = MarqueeInbox.KeyFor(Published(), context, AuthoringStore);
        _other = MarqueeInbox.KeyFor(Revised(), context, AuthoringStore);
    }

    [Fact] void should_give_the_same_key() => _second.ShouldEqual(_first);

    [Fact] void should_give_another_event_another_key() => _other.ShouldNotEqual(_first);
}
#endif
