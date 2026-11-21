// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_a_failure_cooldown_is_followed_by_a_successful_run : a_watchdog_run
{
    void Establish() => _state.Current = StateWith("watchdog:run-failed", TimeSpan.FromMinutes(10));

    async Task Because() => _outcome = await CreateRun().Run(CancellationToken.None);

    [Fact] void should_succeed() => _outcome.Succeeded.ShouldBeTrue();
    [Fact] void should_keep_the_failure_cooldown_so_a_flapping_run_does_not_alert_every_time() => _state.Current.LastAlerted.ContainsKey("watchdog:run-failed").ShouldBeTrue();
    [Fact] void should_not_alert() => _channel.Delivered.ShouldBeEmpty();
}
#endif
