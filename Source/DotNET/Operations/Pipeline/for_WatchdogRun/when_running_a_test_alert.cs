// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_running_a_test_alert : a_watchdog_run
{
    StubCheck _check;

    void Establish()
    {
        _check = StubCheck.Finding("check");
        _checks.Add(_check);
    }

    async Task Because() => _outcome = await CreateRun().RunTestAlert(CancellationToken.None);

    [Fact] void should_succeed() => _outcome.Succeeded.ShouldBeTrue();
    [Fact] void should_deliver_a_test_notification() => Notification.IsTest.ShouldBeTrue();
    [Fact] void should_label_the_subject_as_a_test() => Notification.Subject.ShouldEqual("[shop] TEST ALERT — delivery verification");
    [Fact] void should_not_read_the_state() => _state.Loads.ShouldEqual(0);
    [Fact] void should_not_write_the_state() => _state.Saves.ShouldEqual(0);
    [Fact] void should_not_ping_the_heartbeat() => _heartbeat.Pings.ShouldBeEmpty();
    [Fact] void should_not_claim_that_health_checks_ran() => _check.Passes.ShouldEqual(0);
}
#endif
