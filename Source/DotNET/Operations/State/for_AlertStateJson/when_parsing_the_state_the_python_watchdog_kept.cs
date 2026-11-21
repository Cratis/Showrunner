// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Operations.State.for_AlertStateJson;

public class when_parsing_the_state_the_python_watchdog_kept : Specification
{
    const string Json = """{"pod:shop/web:Pending":1790000000.5,"legacy:only-alerted":1789990000,"__last_check":1790000100,"__first_seen":{"pod:shop/web:Pending":1789900000},"__summary_streaks":{"batch:node-1:summary":2},"__reported_failed_jobs":["watchdog-1","watchdog-2"],"__actions_audit":{"Cratis/Infrastructure":{"build":7}}}""";

    AlertStateSnapshot _snapshot;

    void Because() => _snapshot = AlertStateJson.Parse(Json);

    [Fact] void should_read_when_each_finding_was_last_alerted() => _snapshot.LastAlerted["pod:shop/web:Pending"].ToUnixTimeMilliseconds().ShouldEqual(1790000000500);
    [Fact] void should_keep_findings_that_only_have_an_alert_stamp() => _snapshot.LastAlerted.ContainsKey("legacy:only-alerted").ShouldBeTrue();
    [Fact] void should_not_mistake_reserved_entries_for_findings() => _snapshot.LastAlerted.Keys.ShouldContainOnly("pod:shop/web:Pending", "legacy:only-alerted");
    [Fact] void should_read_when_each_finding_was_first_seen() => _snapshot.FirstSeen["pod:shop/web:Pending"].ToUnixTimeSeconds().ShouldEqual(1789900000);
    [Fact] void should_read_when_the_last_run_started() => _snapshot.LastCheck!.Value.ToUnixTimeSeconds().ShouldEqual(1790000100);
    [Fact] void should_read_the_summary_streaks() => _snapshot.SummaryStreaks["batch:node-1:summary"].ShouldEqual(2);
    [Fact] void should_read_the_reported_failed_jobs() => _snapshot.ReportedFailedJobs.ShouldContainOnly("watchdog-1", "watchdog-2");
    [Fact] void should_keep_the_actions_audit_verbatim() => _snapshot.ActionsAudit!.ShouldContain("\"build\":7");
}
#endif
