// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Link.Protocol;

namespace Cratis.Showrunner.Operations.Link.for_DirectLink;

public class when_direct_is_unreachable_and_returns : a_direct_link
{
    LinkStatus _offline;

    async Task Because()
    {
        await Pass();
        _direct.Unreachable = true;
        await Pass(Failing("pod:shop/web:CrashLoopBackOff"));
        await Pass(Failing("pod:shop/web:CrashLoopBackOff"));
        _offline = Status;
        _direct.Unreachable = false;
        await Pass(Failing("pod:shop/web:CrashLoopBackOff"));
    }

    [Fact] void should_report_offline_and_queue_while_it_lasted() => (_offline.Connection, _offline.OutboxDepth > 0).ShouldEqual((LinkConnection.Offline, true));
    [Fact] void should_deliver_everything_once_it_is_back() => Status.OutboxDepth.ShouldEqual(0);
    [Fact] void should_deliver_in_order_without_a_gap() => _direct.Events.Select(envelope => envelope.Sequence).ShouldEqual(Enumerable.Range(1, _direct.Events.Count).Select(sequence => (long)sequence));
    [Fact] void should_not_have_recorded_a_gap() => _direct.Gaps.ShouldBeEmpty();
    [Fact] void should_hold_an_alert_for_the_failing_check() => _direct.OpenAlerts.ShouldContain("pod:shop/web:CrashLoopBackOff");
    [Fact] void should_report_the_check_against_the_configured_resource() => Sent<CheckFailed>().First().Resource.ShouldEqual(Resource);
}
#endif
