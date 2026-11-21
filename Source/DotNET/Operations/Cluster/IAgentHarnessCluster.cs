// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// The cluster operations the agent harness watchdog is allowed. It deletes worker Jobs, with preconditions, and nothing else.
/// A failure throws <see cref="ClusterReadException"/>, with the HTTP status when the API answered.
/// </summary>
public interface IAgentHarnessCluster
{
    /// <summary>
    /// Lists the worker Jobs that carry the labels.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <param name="labels">The labels a Job must carry.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The Jobs.</returns>
    Task<IReadOnlyList<AgentJob>> ListJobs(string @namespace, IReadOnlyDictionary<string, string> labels, CancellationToken cancellationToken);

    /// <summary>
    /// Reads a Job afresh.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <param name="name">The Job name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The Job.</returns>
    Task<AgentJob> ReadJob(string @namespace, string name, CancellationToken cancellationToken);

    /// <summary>
    /// Lists every Pod that has an owner reference to the Job UID. There is no name or label selector: even a mislabeled Pod counts.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <param name="jobUid">The Job UID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The Pods, ordered by UID.</returns>
    Task<IReadOnlyList<AgentPod>> PodsOwnedBy(string @namespace, string jobUid, CancellationToken cancellationToken);

    /// <summary>
    /// Lists the events about a Pod.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <param name="podUid">The Pod UID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The events.</returns>
    Task<IReadOnlyList<AgentEvent>> PodEvents(string @namespace, string podUid, CancellationToken cancellationToken);

    /// <summary>
    /// Tells whether a Secret exists. An implementation never reads, returns or logs the Secret's data.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <param name="name">The Secret name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> when the Secret exists.</returns>
    Task<bool> SecretExists(string @namespace, string name, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a Job in the foreground, only if it still has that UID and resource version.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <param name="job">The Job as it was verified.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the deletion was requested.</returns>
    Task DeleteJob(string @namespace, AgentJob job, CancellationToken cancellationToken);
}
