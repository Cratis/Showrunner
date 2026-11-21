// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Link.Protocol;

namespace Cratis.Showrunner.Operations.Link.for_Outbox;

public class when_direct_acknowledges_a_watermark : Specification
{
    Outbox _restored;

    void Because()
    {
        var outbox = new Outbox("prod", OutboxSnapshot.Empty, 10, new ManualTimeProvider());
        outbox.Enqueue(new CheckFailed("a", Guid.Empty, 0, "r", 1, 0));
        outbox.Enqueue(new CheckFailed("b", Guid.Empty, 0, "r", 1, 0));
        outbox.Enqueue(new CheckFailed("c", Guid.Empty, 0, "r", 1, 0));
        outbox.Acknowledge(2);

        // A new pass is a new process: the outbox comes back from its saved form and goes on from where it was.
        _restored = new Outbox("prod", outbox.ToSnapshot(), 10, new ManualTimeProvider());
        _restored.Enqueue(new Heartbeat(0, "1", true, 0));
    }

    [Fact] void should_drop_what_direct_took_and_continue_the_sequence_after_a_restart() => _restored.Pending(10).Select(envelope => envelope.Sequence).ShouldEqual([3L, 4L]);
    [Fact] void should_remember_the_watermark() => _restored.Watermark.ShouldEqual(2);
}
#endif
