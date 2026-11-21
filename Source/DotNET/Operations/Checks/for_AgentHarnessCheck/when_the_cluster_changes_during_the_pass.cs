// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_AgentHarnessCheck;

public class when_the_cluster_changes_during_the_pass : an_agent_harness
{
    readonly List<(string Case, AgentHarnessCounts Counts, int Deleted)> _outcomes = [];

    async Task Because()
    {
        // The second Pod listing is the re-read right before the delete decision.
        var races = new (string Name, Action Change)[]
        {
            ("the Job was replaced", () => Mutate(job => job with { Uid = "replacement" })),
            ("the Job changed", () => Mutate(job => job with { ResourceVersion = "changed" })),
            ("the Pod started", () => MutatePod(pod => pod with { Phase = "Running" })),
            ("the Pod changed", () => MutatePod(pod => pod with { ResourceVersion = "new" })),
            ("another Pod appeared", () => _cluster.Pods.Add(Pod(name: "second", uid: "second-pod"))),
            ("the Secret came back", () => _cluster.SecretPresent = true),
            ("the events vanished", () => _cluster.Events.Clear())
        };

        foreach (var (name, change) in races)
        {
            Reset();
            _cluster.OnPodList = (2, change);
            await Scan();
            _outcomes.Add((name, _counts, _cluster.Deleted.Count));
        }
    }

    [Fact] void should_delete_none_of_them_and_never_retry_by_name() => _outcomes.Where(outcome => outcome.Deleted != 0).Select(outcome => outcome.Case).ShouldBeEmpty();
    [Fact] void should_protect_the_job_in_every_one_of_them() => _outcomes.Where(outcome => outcome.Counts.Protected != 1).Select(outcome => outcome.Case).ShouldBeEmpty();
    [Fact] void should_have_tried_every_race() => _outcomes.Count.ShouldEqual(7);
}
#endif
