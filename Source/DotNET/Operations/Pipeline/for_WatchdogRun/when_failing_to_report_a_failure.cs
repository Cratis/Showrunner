// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_failing_to_report_a_failure : a_watchdog_run
{
    void Establish()
    {
        _state.FailLoading = true;
        _state.FailSaving = true;
        _channel.Fail = true;
        _heartbeat.Fail = true;
    }

    async Task Because() => _outcome = await CreateRun().Run(CancellationToken.None);

    [Fact] void should_still_fail_the_run() => _outcome.Succeeded.ShouldBeFalse();
    [Fact] void should_not_let_the_failed_alert_hide_the_original_failure() => _outcome.Failure!.Message.ShouldContain("alert delivery failed");
    [Fact] void should_have_tried_the_heartbeat() => _heartbeat.Pings.ShouldContain(true);
}
#endif
