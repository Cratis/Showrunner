// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_a_finding_crosses_the_escalation_threshold : a_watchdog_run
{
    void Establish()
    {
        FindsEveryPass(new Finding("node:n1:NotReady", "node n1 is not Ready"));

        // Alerted two hours ago, well inside the six-hour cooldown, but opened 25 hours ago: the escalation bypasses the cooldown once.
        _state.Current = StateWith("node:n1:NotReady", TimeSpan.FromHours(2), TimeSpan.FromHours(25));
    }

    async Task Because() => _outcome = await CreateRun().Run(CancellationToken.None);

    [Fact] void should_alert_despite_the_cooldown() => _channel.Delivered.Count.ShouldEqual(1);
    [Fact] void should_escalate_the_finding() => Notification.Escalated.Select(finding => finding.Key).ShouldContainOnly("node:n1:NotReady");
    [Fact] void should_prefix_the_subject() => Notification.Subject.ShouldContain("ESCALATED: ");
    [Fact] void should_list_the_escalated_issues_first() => Notification.Body.ShouldContain("Escalated issues:");
    [Fact] void should_restart_the_cooldown() => _state.Current.LastAlerted["node:n1:NotReady"].ShouldEqual(_time.Now);
}
#endif
