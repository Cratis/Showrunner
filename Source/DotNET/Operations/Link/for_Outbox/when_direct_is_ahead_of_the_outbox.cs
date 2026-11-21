// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Link.Protocol;

namespace Cratis.Showrunner.Operations.Link.for_Outbox;

public class when_direct_is_ahead_of_the_outbox : Specification
{
    Outbox _outbox;

    void Because()
    {
        _outbox = new Outbox("prod", OutboxSnapshot.Empty, 10, new ManualTimeProvider());
        _outbox.Enqueue(new CheckFailed("a", Guid.Empty, 0, "r", 1, 0));
        _outbox.Enqueue(new CheckFailed("b", Guid.Empty, 0, "r", 1, 0));
        _outbox.Rebase(40);
    }

    [Fact] void should_renumber_what_it_holds_above_directs_watermark() => _outbox.Pending(10).Select(envelope => envelope.Sequence).ShouldEqual([41L, 42L]);
    [Fact] void should_continue_after_the_renumbered_entries() => _outbox.LastAssigned.ShouldEqual(42);
    [Fact] void should_be_baselined() => _outbox.Baselined.ShouldBeTrue();
}
#endif
