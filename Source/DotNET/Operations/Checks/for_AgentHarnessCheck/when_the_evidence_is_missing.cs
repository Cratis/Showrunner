// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_AgentHarnessCheck;

public class when_the_evidence_is_missing : an_agent_harness
{
    readonly List<(string Case, AgentHarnessCounts Counts, int Deleted)> _outcomes = [];

    async Task Because()
    {
        var cases = new (string Name, Action Spoil)[]
        {
            ("there are no events", () => _cluster.Events.Clear()),
            ("there are no Pods", () => _cluster.Pods.Clear()),
            ("the event was seen once", () => MutateEvent(item => item with { Count = 1 })),
            ("the event is old", () => MutateEvent(item => item with { Latest = Old })),
            ("the event has no time", () => MutateEvent(item => item with { Latest = null })),
            ("the event is about another Pod", () => MutateEvent(item => item with { InvolvedUid = "other-pod" })),
            ("the event is about another kind", () => MutateEvent(item => item with { InvolvedKind = "Node" })),
            ("the event has another reason", () => MutateEvent(item => item with { Reason = "Failed" })),
            ("the event names another Secret", () => MutateEvent(item => item with { Message = "secret \"startup-other\" not found" })),
            ("the Secret is optional", () => MutatePod(pod => pod with { RequiredSecrets = [] })),
            ("the Pod has no creation time", () => MutatePod(pod => pod with { Created = null })),
            ("the Job has no creation time", () => Mutate(job => job with { Created = null })),
            ("the Secret exists", () => _cluster.SecretPresent = true),
            ("the Secret cannot be checked", () => _cluster.Failing["SecretExists"] = null),
            ("the Pod belongs to another Job", () => MutatePod(pod => pod with { Owners = [pod.Owners[0] with { Uid = "other-job" }] }))
        };

        foreach (var (name, spoil) in cases)
        {
            Reset();
            spoil();
            await Scan();
            _outcomes.Add((name, _counts, _cluster.Deleted.Count));
        }
    }

    [Fact] void should_not_know_what_to_do_in_any_of_them() => _outcomes.Where(outcome => outcome.Counts.Unknown != 1).Select(outcome => outcome.Case).ShouldBeEmpty();
    [Fact] void should_delete_none_of_them() => _outcomes.Where(outcome => outcome.Deleted != 0).Select(outcome => outcome.Case).ShouldBeEmpty();
    [Fact] void should_have_tried_every_case() => _outcomes.Count.ShouldEqual(15);
}
#endif
