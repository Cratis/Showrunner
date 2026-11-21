// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_an_escalated_finding_would_wait_longer_than_a_day : a_watchdog_run
{
    void Establish()
    {
        _options = _options with { RealertInterval = TimeSpan.FromDays(2) };
        FindsEveryPass(new Finding("node:n1:NotReady", "node n1 is not Ready"));

        // Open for three days and last alerted 25 hours ago: the two-day cooldown is capped at the one-day escalation interval.
        _state.Current = StateWith("node:n1:NotReady", TimeSpan.FromHours(25), TimeSpan.FromDays(3));
    }

    async Task Because() => _outcome = await CreateRun().Run(CancellationToken.None);

    [Fact] void should_alert_again() => Notification.Escalated.Select(finding => finding.Key).ShouldContainOnly("node:n1:NotReady");
}
#endif
