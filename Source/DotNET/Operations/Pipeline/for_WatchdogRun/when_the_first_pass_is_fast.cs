// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_the_first_pass_is_fast : a_watchdog_run
{
    StubCheck _check;

    void Establish()
    {
        _check = StubCheck.Finding("check", new Finding("a", "a"));
        _checks.Add(_check);
    }

    async Task Because() => _outcome = await CreateRun().Run(CancellationToken.None);

    [Fact] void should_look_again_before_alerting() => _check.Passes.ShouldEqual(2);
    [Fact] void should_alert() => _channel.Delivered.Count.ShouldEqual(1);
}
#endif
