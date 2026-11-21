// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_AgentHarnessCheck;

public class when_cleanup_is_disabled_or_nothing_is_wrong : an_agent_harness
{
    AgentHarnessCounts _observed;
    int _deletedWhenObserving;
    IReadOnlyCollection<Alerts.Finding> _observerFindings;
    AgentHarnessCounts _idle;
    AgentHarnessCounts _withExit;
    AgentHarnessCounts _withSibling;
    IReadOnlyCollection<Alerts.Finding> _idleFindings;

    async Task Because()
    {
        _options = _options with { CleanupEnabled = false };
        await Scan(cleanup: false);
        _observed = _counts;
        _deletedWhenObserving = _cluster.Deleted.Count;
        _observerFindings = await Check.Collect(CheckContexts.At(_time, null, null), CancellationToken.None);

        Reset();
        _cluster.Jobs.Clear();
        await Scan();
        _idle = _counts;
        _idleFindings = await Check.Collect(CheckContexts.At(_time, null, null), CancellationToken.None);

        Reset();
        MutatePod(pod => pod with { ExitedNonZero = true });
        await Scan();
        _withExit = _counts;

        // A running sibling with an unrelated name and no labels still protects the worker.
        Reset();
        _cluster.Pods.Add(Pod("unrelated-name", "sibling") with { Phase = "Running", Labels = new Dictionary<string, string>() });
        await Scan();
        _withSibling = _counts;
    }

    [Fact] void should_only_observe_when_cleanup_is_disabled() => _deletedWhenObserving.ShouldEqual(0);
    [Fact] void should_count_the_candidate_as_protected() => (_observed.Candidates, _observed.Protected).ShouldEqual((1, 1));
    [Fact] void should_report_the_candidate_so_somebody_can_decide() => _observerFindings.Select(finding => finding.Key).ShouldContainOnly("agent-harness:candidates");
    [Fact] void should_count_nothing_when_there_are_no_worker_jobs() => _idle.ShouldEqual(new AgentHarnessCounts());
    [Fact] void should_report_nothing_when_there_are_no_worker_jobs() => _idleFindings.ShouldBeEmpty();
    [Fact] void should_count_a_container_that_exited_with_a_failing_code() => _withExit.Exited.ShouldEqual(1);
    [Fact] void should_never_delete_a_job_that_has_a_running_sibling_pod() => (_withSibling.Protected, _withSibling.Running, _withSibling.Deleted).ShouldEqual((1, 1, 0));
}
#endif
