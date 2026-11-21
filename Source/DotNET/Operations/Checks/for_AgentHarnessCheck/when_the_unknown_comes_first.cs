// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_AgentHarnessCheck;

public class when_the_unknown_comes_first : an_agent_harness
{
    readonly List<(AgentHarnessCounts Counts, int Deleted)> _outcomes = [];

    async Task Because()
    {
        foreach (var status in new int?[] { null, 500 })
        {
            Reset();
            _cluster.Jobs.Add(Job("worker-two", "job-two"));
            _cluster.Pods.Add(Pod("pod-two", "pod-two", "worker-two", "job-two"));
            _cluster.Events.Add(MissingSecret("pod-two"));
            _cluster.Failing["ReadJob:worker"] = status;
            await Scan();
            _outcomes.Add((_counts, _cluster.Deleted.Count));
        }
    }

    [Fact] void should_not_delete_the_remaining_job_after_an_unknown_one() => _outcomes.Select(outcome => outcome.Deleted).ShouldContainOnly(0, 0);
    [Fact] void should_count_one_unknown_and_both_jobs_as_protected() => _outcomes.Select(outcome => (outcome.Counts.Unknown, outcome.Counts.Protected)).ShouldContainOnly((1, 2), (1, 2));
}
#endif
