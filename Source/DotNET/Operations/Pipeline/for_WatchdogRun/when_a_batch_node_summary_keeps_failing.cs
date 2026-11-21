// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.State;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_a_batch_node_summary_keeps_failing : a_watchdog_run
{
    const string Key = "batch-node:b1:summary";
    AlertStateSnapshot _afterFirst;
    AlertStateSnapshot _afterSecond;

    void Establish() => FindsEveryPass(new Finding(Key, "could not read kubelet summary for node b1", AlertSeverity.Warning));

    async Task Because()
    {
        await CreateRun().Run(CancellationToken.None);
        _afterFirst = _state.Current;
        await CreateRun().Run(CancellationToken.None);
        _afterSecond = _state.Current;
        await CreateRun().Run(CancellationToken.None);
    }

    [Fact] void should_count_the_first_failed_run() => _afterFirst.SummaryStreaks[Key].ShouldEqual(1);
    [Fact] void should_count_the_second_failed_run() => _afterSecond.SummaryStreaks[Key].ShouldEqual(2);
    [Fact] void should_not_alert_before_the_third_consecutive_run() => _channel.Delivered.Count.ShouldEqual(1);
    [Fact] void should_alert_on_the_third_consecutive_run() => Notification.Raised.Select(finding => finding.Key).ShouldContain(Key);
    [Fact] void should_keep_counting() => _state.Current.SummaryStreaks[Key].ShouldEqual(3);
}
#endif
