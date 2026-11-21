// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_AgentHarnessCheck;

public class when_a_job_looks_started_terminal_or_unscoped : an_agent_harness
{
    readonly List<(string Case, AgentHarnessCounts Counts, int Deleted)> _outcomes = [];

    async Task Because()
    {
        var cases = new (string Name, Action Spoil)[]
        {
            ("the Pod is running", () => MutatePod(pod => pod with { Phase = "Running" })),
            ("the Job is suspended", () => Mutate(job => job with { Suspended = true })),
            ("the back-off limit is not zero", () => Mutate(job => job with { BackoffLimit = 1 })),
            ("the Job is being deleted", () => Mutate(job => job with { Deleting = true })),
            ("the Job lacks the managed-by label", () => Mutate(job => job with { Labels = new Dictionary<string, string>() })),
            ("the Pod lacks the worker label", () => MutatePod(pod => pod with { Labels = JobLabels })),
            ("the Job is finished", () => Mutate(job => job with { Finished = true })),
            ("the Pod is being deleted", () => MutatePod(pod => pod with { Deleting = true })),
            ("the owner is not the controller", () => MutatePod(pod => pod with { Owners = [pod.Owners[0] with { Controller = false }] })),
            ("the Pod is new", () => MutatePod(pod => pod with { Created = Recent })),
            ("the Job is new", () => Mutate(job => job with { Created = Recent })),
            ("a container started", () => MutatePod(pod => pod with { Executed = true })),
            ("the controller is not a Job", () => MutatePod(pod => pod with { Owners = [pod.Owners[0] with { Kind = "ReplicaSet" }] })),
            ("the controller has another API version", () => MutatePod(pod => pod with { Owners = [pod.Owners[0] with { ApiVersion = "v1" }] })),
            ("the controller has another name", () => MutatePod(pod => pod with { Owners = [pod.Owners[0] with { Name = "other" }] })),
            ("there are two controllers", () => MutatePod(pod => pod with { Owners = [pod.Owners[0], pod.Owners[0] with { Uid = "second" }] }))
        };

        foreach (var (name, spoil) in cases)
        {
            Reset();
            spoil();
            await Scan();
            _outcomes.Add((name, _counts, _cluster.Deleted.Count));
        }
    }

    [Fact] void should_protect_every_one_of_them() => _outcomes.Where(outcome => outcome.Counts.Protected != 1).Select(outcome => outcome.Case).ShouldBeEmpty();
    [Fact] void should_delete_none_of_them() => _outcomes.Where(outcome => outcome.Deleted != 0).Select(outcome => outcome.Case).ShouldBeEmpty();
    [Fact] void should_not_be_unsure_about_any_of_them() => _outcomes.Where(outcome => outcome.Counts.Unknown != 0).Select(outcome => outcome.Case).ShouldBeEmpty();
    [Fact] void should_have_tried_every_case() => _outcomes.Count.ShouldEqual(16);
}
#endif
