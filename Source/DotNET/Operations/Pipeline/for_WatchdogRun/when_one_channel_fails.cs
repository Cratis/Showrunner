// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_one_channel_fails : a_watchdog_run
{
    RecordingChannel _failing;

    void Establish()
    {
        _failing = new RecordingChannel("failing") { Fail = true };
        _channels.Insert(0, _failing);
        FindsEveryPass(new Finding("a", "a"));
    }

    async Task Because() => _outcome = await CreateRun().Run(CancellationToken.None);

    [Fact] void should_still_deliver_to_the_other_channel() => _channel.Delivered.Count.ShouldEqual(1);
    [Fact] void should_not_fail_the_run() => _outcome.Succeeded.ShouldBeTrue();
    [Fact] void should_advance_the_cooldown() => _state.Current.LastAlerted.ContainsKey("a").ShouldBeTrue();
}
#endif
