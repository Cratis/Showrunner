// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.State;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_the_watchdog_is_a_little_late : a_watchdog_run
{
    void Establish()
    {
        _options = _options with { ScheduleInterval = TimeSpan.FromMinutes(5) };
        _state.Current = AlertStateSnapshot.Empty with { LastCheck = _time.Now - TimeSpan.FromMinutes(14) };
    }

    async Task Because() => _outcome = await CreateRun().Run(CancellationToken.None);

    [Fact] void should_not_report_a_gap_under_three_intervals() => _channel.Delivered.ShouldBeEmpty();
}
#endif
