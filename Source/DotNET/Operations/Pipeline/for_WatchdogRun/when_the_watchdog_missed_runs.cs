// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.State;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_the_watchdog_missed_runs : a_watchdog_run
{
    void Establish()
    {
        _options = _options with { ScheduleInterval = TimeSpan.FromMinutes(5) };
        _state.Current = AlertStateSnapshot.Empty with { LastCheck = _time.Now - TimeSpan.FromMinutes(16) };
    }

    async Task Because() => _outcome = await CreateRun().Run(CancellationToken.None);

    [Fact] void should_report_the_gap() => Notification.Raised.Single().Text.ShouldContain("watchdog missed runs: 960s since last check");
}
#endif
