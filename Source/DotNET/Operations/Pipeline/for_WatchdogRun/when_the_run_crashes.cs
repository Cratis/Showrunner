// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_the_run_crashes : a_watchdog_run
{
    void Establish()
    {
        _options = _options with { WatchdogJobName = "watchdog-29000" };
        _state.FailSaving = true;
    }

    async Task Because() => _outcome = await CreateRun().Run(CancellationToken.None);

    [Fact] void should_fail_with_the_original_exception() => _outcome.Failure.ShouldBeOfExactType<InvalidOperationException>();
    [Fact] void should_alert_that_the_run_failed() => Notification.Raised.Single().Key.ShouldEqual("watchdog:run-failed");
    [Fact] void should_ping_the_heartbeat_as_failed() => _heartbeat.Pings.ShouldContainOnly(true);
    [Fact] void should_describe_the_failure() => Notification.Raised.Single().Text.ShouldContain("watchdog run failed: InvalidOperationException");
}
#endif
