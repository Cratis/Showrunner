// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;

namespace Cratis.Showrunner.Operations.given;

/// <summary>
/// An <see cref="IAgentHarnessCluster"/> the specifications fill in, with hooks to change the cluster in the middle of a pass.
/// </summary>
public sealed class FakeAgentHarnessCluster : IAgentHarnessCluster
{
    /// <summary>
    /// Gets the Jobs.
    /// </summary>
    public List<AgentJob> Jobs { get; } = [];

    /// <summary>
    /// Gets the Pods.
    /// </summary>
    public List<AgentPod> Pods { get; } = [];

    /// <summary>
    /// Gets the events.
    /// </summary>
    public List<AgentEvent> Events { get; } = [];

    /// <summary>
    /// Gets the Jobs that were deleted.
    /// </summary>
    public List<AgentJob> Deleted { get; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether the Secret exists.
    /// </summary>
    public bool SecretPresent { get; set; }

    /// <summary>
    /// Gets how many times the Pods were listed.
    /// </summary>
    public int PodLists { get; private set; }

    /// <summary>
    /// Gets how many times a Job was read.
    /// </summary>
    public int JobReads { get; private set; }

    /// <summary>
    /// Gets how many times a Secret was asked about.
    /// </summary>
    public int SecretReads { get; private set; }

    /// <summary>
    /// Gets or sets what happens when the Pods are listed the given time, to simulate a race.
    /// </summary>
    public (int Time, Action Change)? OnPodList { get; set; }

    /// <summary>
    /// Gets or sets what happens on a Job read, given how many reads came before it.
    /// </summary>
    public Action<int>? OnJobRead { get; set; }

    /// <summary>
    /// Gets or sets what happens on a Secret read, given how many came before it.
    /// </summary>
    public Action<int>? OnSecretRead { get; set; }

    /// <summary>
    /// Gets the operations that fail, by name: <c>ListJobs</c>, <c>ReadJob:worker</c>, <c>PodsOwnedBy</c>, <c>PodEvents</c>, <c>SecretExists</c>
    /// or <c>DeleteJob</c>, with the HTTP status or <see langword="null"/> for a failure without one.
    /// </summary>
    public Dictionary<string, int?> Failing { get; } = [];

    /// <summary>
    /// Gets the namespaces that were asked about.
    /// </summary>
    public HashSet<string> Namespaces { get; } = [];

    /// <inheritdoc/>
    public Task<IReadOnlyList<AgentJob>> ListJobs(string @namespace, IReadOnlyDictionary<string, string> labels, CancellationToken cancellationToken)
    {
        Namespaces.Add(@namespace);
        Fail("ListJobs");
        return Task.FromResult<IReadOnlyList<AgentJob>>([.. Jobs]);
    }

    /// <inheritdoc/>
    public Task<AgentJob> ReadJob(string @namespace, string name, CancellationToken cancellationToken)
    {
        Fail($"ReadJob:{name}");
        OnJobRead?.Invoke(JobReads);
        JobReads++;
        return Task.FromResult(Jobs.Single(job => job.Name == name));
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<AgentPod>> PodsOwnedBy(string @namespace, string jobUid, CancellationToken cancellationToken)
    {
        Fail("PodsOwnedBy");
        PodLists++;
        if (OnPodList is { } race && race.Time == PodLists)
        {
            race.Change();
        }

        return Task.FromResult<IReadOnlyList<AgentPod>>([.. Pods.Where(pod => pod.Owners.Any(owner => owner.Uid == jobUid)).OrderBy(pod => pod.Uid, StringComparer.Ordinal)]);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<AgentEvent>> PodEvents(string @namespace, string podUid, CancellationToken cancellationToken)
    {
        Fail("PodEvents");
        return Task.FromResult<IReadOnlyList<AgentEvent>>([.. Events.Where(item => item.InvolvedUid == podUid)]);
    }

    /// <inheritdoc/>
    public Task<bool> SecretExists(string @namespace, string name, CancellationToken cancellationToken)
    {
        Fail("SecretExists");
        OnSecretRead?.Invoke(SecretReads);
        SecretReads++;
        return Task.FromResult(SecretPresent);
    }

    /// <inheritdoc/>
    public Task DeleteJob(string @namespace, AgentJob job, CancellationToken cancellationToken)
    {
        Fail("DeleteJob");
        Deleted.Add(job);
        return Task.CompletedTask;
    }

    void Fail(string operation)
    {
        if (Failing.TryGetValue(operation, out var status))
        {
            throw new ClusterReadException("private diagnostic that must not be printed", status);
        }
    }
}
#endif
