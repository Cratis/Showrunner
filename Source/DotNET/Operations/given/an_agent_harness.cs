// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Checks;
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.given;

/// <summary>
/// An agent worker Job that has been Pending for ninety five minutes because the Secret its Pod mounts does not exist, with two
/// recent <c>FailedMount</c> events to prove it. A specification spoils exactly what it needs to.
/// </summary>
public abstract class an_agent_harness : a_check
{
    protected FakeAgentHarnessCluster _cluster;
    protected AgentHarnessOptions _options = new() { Namespace = "direct", CleanupEnabled = true };
    protected AgentHarnessCounts _counts;
    protected TimeSpan _budget = TimeSpan.FromSeconds(120);

    protected override IWatchdogCheck Check => new AgentHarnessCheck(_cluster, _options);

    protected DateTimeOffset Old => _time.Now - TimeSpan.FromMinutes(95);

    protected DateTimeOffset Recent => _time.Now - TimeSpan.FromMinutes(3);

    protected static IReadOnlyDictionary<string, string> JobLabels => new Dictionary<string, string> { ["app.kubernetes.io/managed-by"] = "cratis-ai-agents" };

    protected static IReadOnlyDictionary<string, string> PodLabels => new Dictionary<string, string> { ["app.kubernetes.io/managed-by"] = "cratis-ai-agents", ["cratis.io/agent-workload"] = "worker" };

    protected AgentJob Job(string name = "worker", string uid = "job-uid") =>
        new(name, uid, "1", JobLabels, 0, false, false, false, Old);

    protected AgentPod Pod(string name = "worker-pod", string uid = "pod-uid", string job = "worker", string jobUid = "job-uid") =>
        new(name, uid, "2", PodLabels, Old, "Pending", false, [new AgentOwner("Job", "batch/v1", job, jobUid, true)], ["startup"], false, false);

    protected AgentEvent MissingSecret(string podUid = "pod-uid", string secret = "startup") =>
        new(podUid, "Pod", "FailedMount", 2, Recent, $"MountVolume.SetUp failed: secret \"{secret}\" not found");

    protected void Mutate(Func<AgentJob, AgentJob> job) => _cluster.Jobs[0] = job(_cluster.Jobs[0]);

    protected void MutatePod(Func<AgentPod, AgentPod> pod) => _cluster.Pods[0] = pod(_cluster.Pods[0]);

    protected void MutateEvent(Func<AgentEvent, AgentEvent> change) => _cluster.Events[0] = change(_cluster.Events[0]);

    protected async Task Scan(bool cleanup = true)
    {
        _options = _options with { CleanupEnabled = cleanup };
        _counts = await new AgentHarnessCheck(_cluster, _options).Scan(_time, _time.Now + _budget, CancellationToken.None);
    }

    /// <summary>
    /// Starts over with the stuck Job, for a specification that tries several variations.
    /// </summary>
    protected void Reset()
    {
        _cluster = new FakeAgentHarnessCluster();
        _cluster.Jobs.Add(Job());
        _cluster.Pods.Add(Pod());
        _cluster.Events.Add(MissingSecret());
    }

    void Establish() => Reset();
}
#endif
