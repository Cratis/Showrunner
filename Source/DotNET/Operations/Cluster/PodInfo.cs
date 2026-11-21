// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// What the watchdog reads of a pod.
/// </summary>
/// <param name="Namespace">The namespace.</param>
/// <param name="Name">The name.</param>
/// <param name="Phase">The phase: Pending, Running, Succeeded, Failed.</param>
/// <param name="Created">When it was created.</param>
/// <param name="Containers">The container statuses, init containers included.</param>
/// <param name="Reason">The pod-level status reason, for example <c>Evicted</c>.</param>
/// <param name="Started">When the pod started.</param>
/// <param name="Ready">Whether the pod's Ready condition is true.</param>
/// <param name="PodIp">The pod IP, when it has one.</param>
/// <param name="NodeName">The node it runs on.</param>
/// <param name="DisruptedAt">When its DisruptionTarget condition last changed.</param>
/// <param name="Claims">The names of the persistent volume claims it mounts.</param>
/// <param name="Labels">Its labels.</param>
public sealed record PodInfo(
    string Namespace,
    string Name,
    string Phase,
    DateTimeOffset? Created,
    IReadOnlyList<ContainerInfo> Containers,
    string? Reason = null,
    DateTimeOffset? Started = null,
    bool Ready = true,
    string? PodIp = null,
    string? NodeName = null,
    DateTimeOffset? DisruptedAt = null,
    IReadOnlyList<string>? Claims = null,
    IReadOnlyDictionary<string, string>? Labels = null);
