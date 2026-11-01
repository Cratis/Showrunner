// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Everything that distinguishes one of the workloads of a Performance: the application, AuthProxy and Ante all become a Deployment and a Service from one of these.
/// </summary>
/// <param name="Name">The name of the Deployment and Service.</param>
/// <param name="Component">The component: application, authproxy or ante.</param>
/// <param name="Image">The container image.</param>
/// <param name="Port">The port the container listens on.</param>
/// <param name="Replicas">The number of replicas.</param>
/// <param name="Environment">The environment variables, structure only. Never a resolved value.</param>
/// <param name="SecretMounts">The Secret folders to mount.</param>
/// <param name="ConfigMapMounts">The ConfigMap folders to mount.</param>
/// <param name="ConfigRevision">A revision of the mounted content, so the pods roll when it changes.</param>
/// <param name="Resources">The resource requests and limits, or null for none.</param>
/// <param name="ReadinessPath">An HTTP path that answers when the container is ready, or null to check the port.</param>
/// <param name="KeepServingDuringRollout">Whether the old pod keeps serving until the new one is ready.</param>
/// <param name="StopBeforeRollout">Whether the old pod is stopped before the new one starts, so two never share state.</param>
public record WorkloadDefinition(
    string Name,
    string Component,
    string Image,
    int Port,
    int Replicas,
    IReadOnlyDictionary<string, string> Environment,
    IReadOnlyList<SecretMount> SecretMounts,
    IReadOnlyList<ConfigMapMount> ConfigMapMounts,
    string ConfigRevision,
    V1ResourceRequirements? Resources = null,
    string? ReadinessPath = null,
    bool KeepServingDuringRollout = false,
    bool StopBeforeRollout = false);
