// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Link.Protocol;

namespace Cratis.Showrunner.Operations.Link.for_Outbox;

public class when_enqueuing_beyond_the_limit : Specification
{
    Outbox _outbox;

    void Because()
    {
        _outbox = new Outbox("prod", OutboxSnapshot.Empty, 3, new ManualTimeProvider());
        _outbox.Enqueue(new CheckFailed("a", Guid.Empty, 0, "r", 1, 0));
        _outbox.Enqueue(new Heartbeat(0, "1", true, 1));
        _outbox.Enqueue(new CheckFailed("b", Guid.Empty, 0, "r", 1, 0));
        _outbox.Enqueue(new CheckFailed("c", Guid.Empty, 0, "r", 1, 0));
        _outbox.Enqueue(new CheckFailed("d", Guid.Empty, 0, "r", 1, 0));
    }

    [Fact] void should_keep_no_more_than_the_limit() => _outbox.Depth.ShouldEqual(3);
    [Fact] void should_count_what_it_evicted() => _outbox.Evicted.ShouldEqual(2);
    [Fact] void should_evict_the_heartbeat_first_then_the_oldest() => _outbox.Pending(10).Select(envelope => envelope.Sequence).ShouldEqual([3L, 4L, 5L]);
    [Fact] void should_never_reuse_a_sequence() => _outbox.LastAssigned.ShouldEqual(5);
}
#endif
