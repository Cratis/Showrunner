// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Showrunner.Operations.Cluster;
using k8s.Models;

namespace Cratis.Showrunner.Operations.Kubernetes;

/// <summary>
/// Maps the Kubernetes client's models, and the JSON of custom resources and kubelet summaries, to the watchdog's own types.
/// </summary>
internal static class KubernetesMapper
{
    /// <summary>
    /// Maps a pod.
    /// </summary>
    /// <param name="pod">The Kubernetes pod.</param>
    /// <returns>The pod as the watchdog reads it.</returns>
    internal static PodInfo Pod(V1Pod pod)
    {
        var statuses = (pod.Status?.ContainerStatuses ?? []).Concat(pod.Status?.InitContainerStatuses ?? []);
        var disrupted = pod.Status?.Conditions?.FirstOrDefault(condition => condition.Type == "DisruptionTarget")?.LastTransitionTime;
        return new PodInfo(
            pod.Metadata.NamespaceProperty,
            pod.Metadata.Name,
            pod.Status?.Phase ?? "Unknown",
            At(pod.Metadata.CreationTimestamp),
            [.. statuses.Select(Container)],
            pod.Status?.Reason,
            At(pod.Status?.StartTime),
            pod.Status?.Conditions?.Any(condition => condition.Type == "Ready" && condition.Status == "True") ?? false,
            pod.Status?.PodIP,
            pod.Spec?.NodeName,
            At(disrupted),
            [.. (pod.Spec?.Volumes ?? []).Where(volume => volume.PersistentVolumeClaim is not null).Select(volume => volume.PersistentVolumeClaim.ClaimName)],
            pod.Metadata.Labels is null ? null : new Dictionary<string, string>(pod.Metadata.Labels, StringComparer.Ordinal));
    }

    /// <summary>
    /// Maps a container status.
    /// </summary>
    /// <param name="status">The Kubernetes container status.</param>
    /// <returns>The container.</returns>
    internal static ContainerInfo Container(V1ContainerStatus status) => new(
        status.Name,
        status.Ready,
        status.RestartCount,
        status.State?.Waiting?.Reason,
        At(status.State?.Terminated?.FinishedAt),
        status.State?.Terminated?.Message);

    /// <summary>
    /// Maps a node.
    /// </summary>
    /// <param name="node">The Kubernetes node.</param>
    /// <returns>The node.</returns>
    internal static NodeInfo Node(V1Node node) => new(
        node.Metadata.Name,
        [.. (node.Status?.Conditions ?? []).Select(condition => new NodeCondition(condition.Type, condition.Status, condition.Reason))]);

    /// <summary>
    /// Maps a CronJob.
    /// </summary>
    /// <param name="cronJob">The Kubernetes CronJob.</param>
    /// <returns>The CronJob.</returns>
    internal static CronJobInfo CronJob(V1CronJob cronJob) =>
        new(cronJob.Metadata.Name, At(cronJob.Metadata.CreationTimestamp), At(cronJob.Status?.LastSuccessfulTime));

    /// <summary>
    /// Maps a Job.
    /// </summary>
    /// <param name="job">The Kubernetes Job.</param>
    /// <returns>The Job.</returns>
    internal static JobInfo Job(V1Job job)
    {
        var failed = job.Status?.Conditions?.FirstOrDefault(condition => condition.Type == "Failed" && condition.Status == "True");
        return new JobInfo(
            job.Metadata.Name,
            At(job.Metadata.CreationTimestamp),
            [.. (job.Metadata.OwnerReferences ?? []).Where(owner => owner.Kind == "CronJob").Select(owner => owner.Name)],
            At(job.Status?.StartTime),
            job.Status?.Conditions?.Any(condition => condition.Type == "Complete" && condition.Status == "True") ?? false,
            failed is not null,
            failed?.Reason,
            failed?.Message);
    }

    /// <summary>
    /// Maps a list of cert-manager Certificates.
    /// </summary>
    /// <param name="list">The JSON of the list.</param>
    /// <returns>The Certificates.</returns>
    internal static IReadOnlyList<CertificateInfo> Certificates(JsonElement list) => [.. Items(list).Select(item =>
    {
        var status = Property(item, "status");
        var ready = Property(status, "conditions") is { ValueKind: JsonValueKind.Array } conditions
            ? conditions.EnumerateArray().FirstOrDefault(condition => Text(condition, "type") == "Ready")
            : default;
        return new CertificateInfo(
            Text(Property(item, "metadata"), "namespace") ?? string.Empty,
            Text(Property(item, "metadata"), "name") ?? string.Empty,
            ready.ValueKind == JsonValueKind.Object ? Text(ready, "status") : null,
            Text(status, "notAfter"));
    })];

    /// <summary>
    /// Maps a list of Percona Backup for MongoDB backups.
    /// </summary>
    /// <param name="list">The JSON of the list.</param>
    /// <returns>The backups.</returns>
    internal static IReadOnlyList<PbmBackupInfo> PbmBackups(JsonElement list) => [.. Items(list).Select(item =>
    {
        var status = Property(item, "status");
        var metadata = Property(item, "metadata");
        return new PbmBackupInfo(
            Text(metadata, "name") ?? string.Empty,
            Text(status, "state"),
            Time(Text(metadata, "creationTimestamp")),
            Time(Text(status, "completed")),
            Text(status, "error"));
    })];

    /// <summary>
    /// Reads the volume usage out of a kubelet stats summary. It throws when the summary has no readable pod usage.
    /// </summary>
    /// <param name="summary">The JSON of the summary.</param>
    /// <returns>The usage of the claims.</returns>
    /// <exception cref="ClusterReadException">The summary has no pod usage.</exception>
    internal static IReadOnlyList<VolumeUsage> VolumeUsage(string summary)
    {
        using var document = JsonDocument.Parse(summary);
        if (Property(document.RootElement, "pods") is not { ValueKind: JsonValueKind.Array } pods)
        {
            throw new ClusterReadException("kubelet summary has no readable pod usage");
        }

        var usage = new List<VolumeUsage>();
        foreach (var pod in pods.EnumerateArray())
        {
            if (Property(pod, "volume") is not { ValueKind: JsonValueKind.Array } volumes)
            {
                continue;
            }

            foreach (var volume in volumes.EnumerateArray())
            {
                var claim = Property(volume, "pvcRef");
                if (claim.ValueKind == JsonValueKind.Object
                    && Text(claim, "name") is { } name
                    && Text(claim, "namespace") is { } @namespace
                    && Number(volume, "capacityBytes") is { } capacity
                    && Number(volume, "usedBytes") is { } used)
                {
                    usage.Add(new VolumeUsage(@namespace, name, capacity, used));
                }
            }
        }

        return usage;
    }

    static JsonElement[] Items(JsonElement list) =>
        Property(list, "items") is { ValueKind: JsonValueKind.Array } items ? [.. items.EnumerateArray()] : [];

    static JsonElement Property(JsonElement element, string name) =>
        element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) ? value : default;

    static string? Text(JsonElement element, string name) =>
        Property(element, name) is { ValueKind: JsonValueKind.String } value ? value.GetString() : null;

    static long? Number(JsonElement element, string name) =>
        Property(element, name) is { ValueKind: JsonValueKind.Number } value && value.TryGetInt64(out var number) ? number : null;

    static DateTimeOffset? Time(string? value) =>
        DateTimeOffset.TryParse(value, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal, out var time) ? time : null;

    static DateTimeOffset? At(DateTime? value) =>
        value is { } time ? new DateTimeOffset(DateTime.SpecifyKind(time, DateTimeKind.Utc)) : null;
}
