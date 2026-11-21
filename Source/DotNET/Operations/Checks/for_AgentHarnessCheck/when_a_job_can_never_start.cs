// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_AgentHarnessCheck;

public class when_a_job_can_never_start : an_agent_harness
{
    async Task Because()
    {
        await Scan();
        await Run();
    }

    [Fact] void should_consider_and_delete_exactly_the_one_job() => (_counts.Considered, _counts.Deleted).ShouldEqual((1, 1));
    [Fact] void should_delete_it_with_the_uid_and_version_that_was_verified() => _cluster.Deleted.Select(job => job.Fingerprint).ShouldContain("job-uid@1");
    [Fact] void should_not_call_a_job_that_was_deleted_protected() => _counts.Protected.ShouldEqual(0);
    [Fact] void should_say_what_it_deleted() => _findings.Single(finding => finding.Key == "agent-harness:deleted").Text.ShouldContain("deleted 1 agent worker Job(s) in direct");
    [Fact] void should_not_alarm_for_a_cleanup() => _findings.Single().Severity.ShouldEqual(Alerts.AlertSeverity.Information);
    [Fact] void should_only_ever_look_in_the_agent_namespace() => _cluster.Namespaces.ShouldContainOnly("direct");
}
#endif
