// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.State;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_a_legacy_state_only_knows_when_a_finding_was_alerted : a_watchdog_run
{
    void Establish()
    {
        FindsEveryPass(new Finding("disk", "disk full"));
        _state.Current = AlertStateSnapshot.Empty with { LastAlerted = AlertStateSnapshot.Empty.LastAlerted.Add("disk", _time.Now - TimeSpan.FromSeconds(100)) };
    }

    async Task Because() => _outcome = await CreateRun().Run(CancellationToken.None);

    [Fact] void should_use_the_alert_time_as_the_first_seen_time() => _state.Current.FirstSeen["disk"].ShouldEqual(_started - TimeSpan.FromSeconds(100));
    [Fact] void should_not_alert_again_inside_the_cooldown() => _channel.Delivered.ShouldBeEmpty();
}
#endif
