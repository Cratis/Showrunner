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
public record WorkloadDefinition(
    string Name,
    string Component,
    string Image,
    int Port,
    int Replicas,
    IReadOnlyDictionary<string, string> Environment,
    IReadOnlyList<SecretMount> SecretMounts,
    IReadOnlyList<ConfigMapMount> ConfigMapMounts,
    string ConfigRevision);
