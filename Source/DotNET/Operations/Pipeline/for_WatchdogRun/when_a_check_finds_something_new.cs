// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Pipeline.for_WatchdogRun;

public class when_a_check_finds_something_new : a_watchdog_run
{
    void Establish() => _checks.Add(StubCheck.Finding("check", new Finding("pod:shop/web:Pending", "shop/web has been Pending", AlertSeverity.Warning)));

    async Task Because() => _outcome = await CreateRun().Run(CancellationToken.None);

    [Fact] void should_succeed() => _outcome.Succeeded.ShouldBeTrue();
    [Fact] void should_exit_with_zero() => _outcome.ExitCode.ShouldEqual(0);
    [Fact] void should_deliver_exactly_one_alert() => _channel.Delivered.Count.ShouldEqual(1);
    [Fact] void should_raise_the_finding() => Notification.Raised.Select(finding => finding.Key).ShouldContainOnly("pod:shop/web:Pending");
    [Fact] void should_escalate_nothing() => Notification.Escalated.ShouldBeEmpty();
    [Fact] void should_say_when_the_finding_was_first_seen() => Notification.Raised.Single().Text.ShouldContain("(open since 2026-07-01 12:00 UTC, 0h)");
    [Fact] void should_count_the_open_issues_in_the_subject() => Notification.Subject.ShouldEqual("[shop] 1 cluster issue(s) need attention");
    [Fact] void should_remember_when_it_was_alerted() => _state.Current.LastAlerted["pod:shop/web:Pending"].ShouldEqual(_time.Now);
    [Fact] void should_remember_when_it_was_first_seen() => _state.Current.FirstSeen["pod:shop/web:Pending"].ShouldEqual(_time.Now);
    [Fact] void should_remember_when_the_run_started() => _state.Current.LastCheck.ShouldEqual(_started);
    [Fact] void should_ping_the_heartbeat_as_healthy() => _heartbeat.Pings.ShouldContainOnly(false);
}
#endif
