// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_the_run_crashes_after_the_failure_cooldown : a_watchdog_run
{
    void Establish()
    {
        _checks.Add(StubCheck.Finding("check"));
        _heartbeat.Fail = true;
        _state.Current = StateWith("watchdog:run-failed", TimeSpan.FromHours(7));
    }

    async Task Because() => _outcome = await CreateRun().Run(CancellationToken.None);

    [Fact] void should_alert_again() => Notification.Raised.Single().Key.ShouldEqual("watchdog:run-failed");
    [Fact] void should_restart_the_cooldown() => _state.Current.LastAlerted["watchdog:run-failed"].ShouldEqual(_time.Now);
}
#endif
