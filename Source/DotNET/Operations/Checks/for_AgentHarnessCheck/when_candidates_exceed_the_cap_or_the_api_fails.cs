// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_AgentHarnessCheck;

public class when_candidates_exceed_the_cap_or_the_api_fails : an_agent_harness
{
    AgentHarnessCounts _tooMany;
    int _deletedWhenTooMany;
    readonly Dictionary<string, (AgentHarnessCounts Counts, int Deleted)> _failures = [];

    async Task Because()
    {
        for (var index = 0; index < 4; index++)
        {
            _cluster.Jobs.Add(Job($"worker-{index}", $"uid-{index}"));
            _cluster.Pods.Add(Pod($"pod-{index}", $"pod-uid-{index}", $"worker-{index}", $"uid-{index}"));
            _cluster.Events.Add(MissingSecret($"pod-uid-{index}"));
        }

        _cluster.Jobs.RemoveAt(0);
        _cluster.Pods.RemoveAt(0);
        _cluster.Events.RemoveAt(0);
        await Scan();
        _tooMany = _counts;
        _deletedWhenTooMany = _cluster.Deleted.Count;

        foreach (var failure in new[] { "ListJobs", "PodsOwnedBy", "PodEvents", "ReadJob:worker", "DeleteJob" })
        {
            Reset();
            _cluster.Failing[failure] = failure == "DeleteJob" ? 500 : null;
            await Scan();
            _failures[failure] = (_counts, _cluster.Deleted.Count);
        }

        Reset();
        _cluster.Failing["DeleteJob"] = 409;
        await Scan();
        _failures["conflict"] = (_counts, _cluster.Deleted.Count);
    }

    [Fact] void should_refuse_to_delete_when_more_than_the_cap_are_candidates() => (_tooMany.Candidates, _deletedWhenTooMany).ShouldEqual((4, 0));
    [Fact] void should_count_the_refusal_as_a_known_failure() => (_tooMany.Failed, _tooMany.Protected, _tooMany.Unknown).ShouldEqual((1, 4, 0));
    [Fact] void should_not_delete_when_listing_jobs_fails() => _failures["ListJobs"].Deleted.ShouldEqual(0);
    [Fact] void should_not_be_sure_when_listing_jobs_fails() => _failures["ListJobs"].Counts.Unknown.ShouldEqual(1);
    [Fact] void should_not_be_sure_when_listing_pods_fails() => _failures["PodsOwnedBy"].Counts.Unknown.ShouldEqual(1);
    [Fact] void should_not_be_sure_when_listing_events_fails() => _failures["PodEvents"].Counts.Unknown.ShouldEqual(1);
    [Fact] void should_not_be_sure_when_rereading_the_job_fails() => _failures["ReadJob:worker"].Counts.Unknown.ShouldEqual(1);
    [Fact] void should_not_be_sure_when_the_delete_fails_with_a_server_error() => _failures["DeleteJob"].Counts.Unknown.ShouldEqual(1);
    [Fact] void should_treat_a_conflict_as_a_known_failure() => (_failures["conflict"].Counts.Unknown, _failures["conflict"].Counts.Failed).ShouldEqual((0, 1));
    [Fact] void should_never_delete_through_any_failure() => _failures.Values.Sum(outcome => outcome.Counts.Deleted).ShouldEqual(0);
}
#endif
