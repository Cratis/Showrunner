// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_every_channel_fails : a_watchdog_run
{
    void Establish()
    {
        _channel.Fail = true;
        FindsEveryPass(new Finding("a", "a"));
    }

    async Task Because() => _outcome = await CreateRun().Run(CancellationToken.None);

    [Fact] void should_fail_the_run() => _outcome.Succeeded.ShouldBeFalse();
    [Fact] void should_say_that_delivery_failed() => _outcome.Failure!.Message.ShouldContain("alert delivery failed");
    [Fact] void should_never_put_the_transport_error_in_the_failure() => _outcome.Failure!.Message.ShouldNotContain("secret-token");
    [Fact] void should_ping_the_heartbeat_as_failed() => _heartbeat.Pings.ShouldContainOnly(true);
    [Fact] void should_not_advance_the_cooldown() => _state.Current.LastAlerted.ContainsKey("a").ShouldBeFalse();
    [Fact] void should_not_save_the_first_seen_time() => _state.Current.FirstSeen.ContainsKey("a").ShouldBeFalse();
}
#endif
