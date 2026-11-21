// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.State;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_no_schedule_is_known : a_watchdog_run
{
    void Establish() => _state.Current = AlertStateSnapshot.Empty with { LastCheck = _time.Now - TimeSpan.FromDays(3) };

    async Task Because() => _outcome = await CreateRun().Run(CancellationToken.None);

    [Fact] void should_not_infer_missed_runs() => _channel.Delivered.ShouldBeEmpty();
}
#endif
