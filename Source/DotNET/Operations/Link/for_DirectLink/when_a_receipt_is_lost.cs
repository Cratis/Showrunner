// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Link.for_DirectLink;

public class when_a_receipt_is_lost : a_direct_link
{
    async Task Because()
    {
        await Pass();
        _direct.DropResponses = true;
        await Pass(Failing("pod:shop/web:CrashLoopBackOff"));
        _direct.DropResponses = false;
        await Pass(Failing("pod:shop/web:CrashLoopBackOff"));
    }

    [Fact] void should_send_the_batch_again() => _direct.Duplicates.ShouldBeGreaterThan(0);
    [Fact] void should_not_record_any_fact_twice() => _direct.Events.Select(envelope => envelope.EventId).Distinct().Count().ShouldEqual(_direct.Events.Count);
    [Fact] void should_keep_the_sequence_contiguous() => _direct.Events.Select(envelope => envelope.Sequence).ShouldEqual(Enumerable.Range(1, _direct.Events.Count).Select(sequence => (long)sequence));
    [Fact] void should_end_with_an_empty_outbox() => Status.OutboxDepth.ShouldEqual(0);
}
#endif
