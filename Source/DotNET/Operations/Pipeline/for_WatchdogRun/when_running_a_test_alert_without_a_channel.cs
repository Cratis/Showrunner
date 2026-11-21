// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_running_a_test_alert_without_a_channel : a_watchdog_run
{
    void Establish() => _channels.Clear();

    async Task Because() => _outcome = await CreateRun().RunTestAlert(CancellationToken.None);

    [Fact] void should_fail_because_nothing_was_delivered() => _outcome.Succeeded.ShouldBeFalse();
    [Fact] void should_say_that_none_is_configured() => _outcome.Failure!.Message.ShouldContain("none configured");
}
#endif
