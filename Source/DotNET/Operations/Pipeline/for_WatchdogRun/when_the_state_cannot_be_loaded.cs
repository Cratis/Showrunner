// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_the_state_cannot_be_loaded : a_watchdog_run
{
    void Establish()
    {
        _state.FailLoading = true;
        FindsEveryPass(new Finding("pod:shop/web:Pending", "shop/web has been Pending"));
    }

    async Task Because() => _outcome = await CreateRun().Run(CancellationToken.None);

    [Fact] void should_still_collect_and_alert() => Notification.Raised.Select(finding => finding.Key).ShouldContain("pod:shop/web:Pending");
    [Fact] void should_report_the_state_outage_as_a_finding() => Notification.Raised.Select(finding => finding.Key).ShouldContain("state:api");
    [Fact] void should_succeed() => _outcome.Succeeded.ShouldBeTrue();
    [Fact] void should_cool_the_run_failure_down_because_the_delivery_covered_the_outage() => _state.Current.LastAlerted.ContainsKey("watchdog:run-failed").ShouldBeTrue();
}
#endif
