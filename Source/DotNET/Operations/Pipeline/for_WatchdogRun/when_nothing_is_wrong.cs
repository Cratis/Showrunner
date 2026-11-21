// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_nothing_is_wrong : a_watchdog_run
{
    StubCheck _check;

    void Establish()
    {
        _check = StubCheck.Finding("check");
        _checks.Add(_check);
    }

    async Task Because() => _outcome = await CreateRun().Run(CancellationToken.None);

    [Fact] void should_succeed() => _outcome.Succeeded.ShouldBeTrue();
    [Fact] void should_not_waste_time_on_a_second_look() => _check.Passes.ShouldEqual(1);
    [Fact] void should_not_alert() => _channel.Delivered.ShouldBeEmpty();
    [Fact] void should_save_the_state_with_the_time_of_the_run() => _state.Current.LastCheck.ShouldEqual(_started);
    [Fact] void should_ping_the_heartbeat_as_healthy() => _heartbeat.Pings.ShouldContainOnly(false);
}
#endif
