// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using Cratis.Showrunner.Operations.Cluster;
using k8s;
using k8s.Autorest;
using k8s.Models;

namespace Cratis.Showrunner.Operations.Kubernetes;

/// <summary>
/// An <see cref="IAgentHarnessCluster"/> that uses the Kubernetes client. It lists Jobs, Pods and events, asks whether a Secret
/// exists (asking for metadata only, and never keeping what comes back), and deletes a Job with preconditions. Nothing else.
/// </summary>
/// <param name="client">The Kubernetes client.</param>
public sealed class KubernetesAgentHarnessCluster(IKubernetes client) : IAgentHarnessCluster
{
    const string MetadataOnly = "application/json;as=PartialObjectMetadata;g=meta.k8s.io;v=v1";
    const int PageSize = 100;

    static readonly string[] _terminalConditions = ["Complete", "Failed", "FailureTarget", "SuccessCriteriaMet"];

    /// <inheritdoc/>
    public Task<IReadOnlyList<AgentJob>> ListJobs(string @namespace, IReadOnlyDictionary<string, string> labels, CancellationToken cancellationToken) =>
        Guard(async () =>
        {
            var selector = string.Join(',', labels.Select(label => $"{label.Key}={label.Value}"));
            var jobs = new List<AgentJob>();
            string? next = null;
            do
            {
                var page = await client.BatchV1.ListNamespacedJobAsync(@namespace, continueParameter: next, labelSelector: selector, limit: PageSize, cancellationToken: cancellationToken);
                jobs.AddRange(page.Items.Select(Job));
                next = string.IsNullOrEmpty(page.Metadata?.ContinueProperty) ? null : page.Metadata.ContinueProperty;
            }
            while (next is not null);
            return (IReadOnlyList<AgentJob>)jobs;
        });

    /// <inheritdoc/>
    public Task<AgentJob> ReadJob(string @namespace, string name, CancellationToken cancellationToken) =>
        Guard(async () => Job(await client.BatchV1.ReadNamespacedJobAsync(name, @namespace, cancellationToken: cancellationToken)));

    /// <inheritdoc/>
    public Task<IReadOnlyList<AgentPod>> PodsOwnedBy(string @namespace, string jobUid, CancellationToken cancellationToken) =>
        Guard(async () =>
        {
            // No name or label selector: every Pod that references this Job UID counts, even a mislabeled one.
            var pods = new List<AgentPod>();
            string? next = null;
            do
            {
                var page = await client.CoreV1.ListNamespacedPodAsync(@namespace, continueParameter: next, limit: PageSize, cancellationToken: cancellationToken);
                pods.AddRange(page.Items.Where(pod => (pod.Metadata.OwnerReferences ?? []).Any(owner => owner.Uid == jobUid)).Select(Pod));
                next = string.IsNullOrEmpty(page.Metadata?.ContinueProperty) ? null : page.Metadata.ContinueProperty;
            }
            while (next is not null);
            return (IReadOnlyList<AgentPod>)[.. pods.OrderBy(pod => pod.Uid, StringComparer.Ordinal)];
        });

    /// <inheritdoc/>
    public Task<IReadOnlyList<AgentEvent>> PodEvents(string @namespace, string podUid, CancellationToken cancellationToken) =>
        Guard(async () =>
        {
            var events = new List<AgentEvent>();
            string? next = null;
            do
            {
                var page = await client.CoreV1.ListNamespacedEventAsync(@namespace, continueParameter: next, fieldSelector: $"involvedObject.uid={podUid}", limit: PageSize, cancellationToken: cancellationToken);
                events.AddRange(page.Items.Select(Event));
                next = string.IsNullOrEmpty(page.Metadata?.ContinueProperty) ? null : page.Metadata.ContinueProperty;
            }
            while (next is not null);
            return (IReadOnlyList<AgentEvent>)events;
        });

    /// <inheritdoc/>
    public async Task<bool> SecretExists(string @namespace, string name, CancellationToken cancellationToken)
    {
        // Existence only: ask for metadata, and never read, keep or log what comes back.
        var headers = new Dictionary<string, IReadOnlyList<string>> { ["Accept"] = [MetadataOnly] };
        try
        {
            using var response = await client.CoreV1.ReadNamespacedSecretWithHttpMessagesAsync(name, @namespace, customHeaders: headers, cancellationToken: cancellationToken);
            return true;
        }
        catch (HttpOperationException error) when (error.Response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
        catch (HttpOperationException error)
        {
            throw new ClusterReadException($"could not check a Secret: HTTP {(int)error.Response.StatusCode}", (int)error.Response.StatusCode);
        }
    }

    /// <inheritdoc/>
    public Task DeleteJob(string @namespace, AgentJob job, CancellationToken cancellationToken) =>
        Guard(async () =>
        {
            var options = new V1DeleteOptions
            {
                PropagationPolicy = "Foreground",
                Preconditions = new V1Preconditions { Uid = job.Uid, ResourceVersion = job.ResourceVersion }
            };
            await client.BatchV1.DeleteNamespacedJobAsync(job.Name, @namespace, options, cancellationToken: cancellationToken);
            return true;
        });

    /// <summary>
    /// Maps a Job.
    /// </summary>
    /// <param name="job">The Kubernetes Job.</param>
    /// <returns>The Job as the watchdog reads it.</returns>
    internal static AgentJob Job(V1Job job) => new(
        job.Metadata.Name,
        job.Metadata.Uid,
        job.Metadata.ResourceVersion,
        new Dictionary<string, string>(job.Metadata.Labels ?? new Dictionary<string, string>()),
        job.Spec?.BackoffLimit,
        job.Spec?.Suspend == true,
        job.Metadata.DeletionTimestamp is not null,
        IsFinished(job),
        At(job.Metadata.CreationTimestamp));

    /// <summary>
    /// Maps a Pod.
    /// </summary>
    /// <param name="pod">The Kubernetes Pod.</param>
    /// <returns>The Pod as the watchdog reads it.</returns>
    internal static AgentPod Pod(V1Pod pod)
    {
        var containers = Containers(pod).ToList();
        return new AgentPod(
            pod.Metadata.Name,
            pod.Metadata.Uid,
            pod.Metadata.ResourceVersion,
            new Dictionary<string, string>(pod.Metadata.Labels ?? new Dictionary<string, string>()),
            At(pod.Metadata.CreationTimestamp),
            pod.Status?.Phase,
            pod.Metadata.DeletionTimestamp is not null,
            [.. (pod.Metadata.OwnerReferences ?? []).Select(owner => new AgentOwner(owner.Kind, owner.ApiVersion, owner.Name, owner.Uid, owner.Controller == true))],
            [.. (pod.Spec?.Volumes ?? []).Where(volume => volume.Secret is not null && volume.Secret.Optional != true).Select(volume => volume.Secret.SecretName)],
            HasExecuted(pod, containers),
            containers.Exists(container => FailedExit(container.State) || FailedExit(container.LastState)));
    }

    /// <summary>
    /// Maps an event.
    /// </summary>
    /// <param name="item">The Kubernetes event.</param>
    /// <returns>The event as the watchdog reads it.</returns>
    internal static AgentEvent Event(Corev1Event item) => new(
        item.InvolvedObject?.Uid ?? string.Empty,
        item.InvolvedObject?.Kind ?? string.Empty,
        item.Reason ?? string.Empty,
        item.Series?.Count ?? item.Count ?? 0,
        At(item.Series?.LastObservedTime ?? item.LastTimestamp ?? item.EventTime),
        item.Message ?? string.Empty);

    static IEnumerable<V1ContainerStatus> Containers(V1Pod pod) =>
        (pod.Status?.ContainerStatuses ?? []).Concat(pod.Status?.InitContainerStatuses ?? []).Concat(pod.Status?.EphemeralContainerStatuses ?? []);

    static int? ExitCode(V1ContainerState? state) => state?.Terminated?.ExitCode;

    static bool IsFinished(V1Job job) =>
        (job.Status?.Succeeded ?? 0) != 0
        || (job.Status?.Failed ?? 0) != 0
        || (job.Status?.Conditions ?? []).Any(condition => string.Equals(condition.Status, "True", StringComparison.Ordinal) && _terminalConditions.Contains(condition.Type));

    static bool FailedExit(V1ContainerState? state) => ExitCode(state) is { } code && code != 0;

    static bool IsReady(V1PodCondition condition) =>
        string.Equals(condition.Status, "True", StringComparison.Ordinal)
        && (string.Equals(condition.Type, "Ready", StringComparison.Ordinal) || string.Equals(condition.Type, "ContainersReady", StringComparison.Ordinal));

    /// <summary>
    /// Tells whether anything shows a container ever started. The Pod start time records kubelet acknowledgment before volume
    /// mounting, not container execution, so it is not evidence.
    /// </summary>
    /// <param name="pod">The Pod.</param>
    /// <param name="containers">Every container status of the Pod.</param>
    /// <returns>True when a container started or the Pod became ready.</returns>
    static bool HasExecuted(V1Pod pod, List<V1ContainerStatus> containers) =>
        (pod.Status?.Conditions ?? []).Any(IsReady)
        || containers.Exists(container =>
            container.Started == true
            || container.Ready
            || !string.IsNullOrEmpty(container.ContainerID)
            || container.RestartCount != 0
            || container.State?.Running is not null
            || container.State?.Terminated is not null
            || container.LastState?.Running is not null
            || container.LastState?.Terminated is not null);

    static DateTimeOffset? At(DateTime? value) =>
        value is { } time ? new DateTimeOffset(DateTime.SpecifyKind(time, DateTimeKind.Utc)) : null;

    static async Task<T> Guard<T>(Func<Task<T>> operation)
    {
        try
        {
            return await operation();
        }
        catch (HttpOperationException error)
        {
            throw new ClusterReadException($"the Kubernetes API answered HTTP {(int)error.Response.StatusCode}", (int)error.Response.StatusCode);
        }
    }
}
