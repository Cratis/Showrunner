// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_running_a_test_alert_and_a_channel_fails : a_watchdog_run
{
    void Establish() => _channels.Insert(0, new RecordingChannel("failing") { Fail = true });

    async Task Because() => _outcome = await CreateRun().RunTestAlert(CancellationToken.None);

    [Fact] void should_still_reach_the_other_channel() => _channel.Delivered.Count.ShouldEqual(1);
    [Fact] void should_fail_the_test_because_partial_delivery_is_not_verified_delivery() => _outcome.Succeeded.ShouldBeFalse();
    [Fact] void should_name_the_channel_that_failed() => _outcome.Failure!.Message.ShouldContain("failing: failed");
}
#endif
