// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_a_finding_has_been_open_exactly_the_escalation_threshold : a_watchdog_run
{
    void Establish()
    {
        // No pause before the re-check, so the clock stands still and the finding is open for exactly 24 hours.
        _options = _options with { RecheckPause = TimeSpan.Zero };
        FindsEveryPass(new Finding("node:n1:NotReady", "node n1 is not Ready"));
        _state.Current = StateWith("node:n1:NotReady", TimeSpan.FromHours(2), TimeSpan.FromHours(24));
    }

    async Task Because() => _outcome = await CreateRun().Run(CancellationToken.None);

    [Fact] void should_not_escalate() => _channel.Delivered.ShouldBeEmpty();
}
#endif
