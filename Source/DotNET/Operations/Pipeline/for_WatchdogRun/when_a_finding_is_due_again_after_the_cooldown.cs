// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_a_finding_is_due_again_after_the_cooldown : a_watchdog_run
{
    void Establish()
    {
        FindsEveryPass(new Finding("node:n1:NotReady", "node n1 is not Ready"));
        _state.Current = StateWith("node:n1:NotReady", TimeSpan.FromHours(7));
    }

    async Task Because() => _outcome = await CreateRun().Run(CancellationToken.None);

    [Fact] void should_alert_again() => Notification.Raised.Select(finding => finding.Key).ShouldContainOnly("node:n1:NotReady");
    [Fact] void should_not_call_it_escalated() => Notification.Escalated.ShouldBeEmpty();
    [Fact] void should_report_how_long_it_has_been_open() => Notification.Raised.Single().Text.ShouldContain("7h)");
    [Fact] void should_restart_the_cooldown() => _state.Current.LastAlerted["node:n1:NotReady"].ShouldEqual(_time.Now);
    [Fact] void should_keep_the_open_since_time() => _state.Current.FirstSeen["node:n1:NotReady"].ShouldEqual(_started - TimeSpan.FromHours(7));
}
#endif
