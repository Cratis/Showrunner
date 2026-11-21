// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Link.for_DirectLink;

public class when_the_outbox_exceeds_its_limit : a_direct_link
{
    LinkStatus _queued;

    void Establish() => _options = _options with { OutboxLimit = 3 };

    async Task Because()
    {
        await Pass();
        _direct.Unreachable = true;
        for (var pass = 0; pass < 4; pass++)
        {
            await Pass(Failing($"pod:a-{pass}"), Failing($"pod:b-{pass}"));
        }

        _queued = Status;
        _direct.Unreachable = false;
        await Pass();
    }

    [Fact] void should_keep_no_more_than_its_limit() => (_queued.OutboxDepth <= 3).ShouldBeTrue();
    [Fact] void should_count_what_it_evicted() => _queued.Evicted.ShouldBeGreaterThan(0L);
    [Fact] void should_let_direct_record_the_gap_instead_of_hiding_it() => _direct.Gaps.ShouldNotBeEmpty();
    [Fact] void should_still_deliver_what_it_kept() => Status.OutboxDepth.ShouldEqual(0);
}
#endif
