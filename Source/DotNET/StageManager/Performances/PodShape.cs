// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// What a workload adds to its pod beyond the Secrets and ConfigMaps it mounts.
/// </summary>
/// <param name="LivenessPath">An HTTP path that answers while the container is alive, or null for no liveness check.</param>
/// <param name="StartupPath">An HTTP path that answers once the container has started, or null for no startup check.</param>
/// <param name="ImagePullSecrets">The names of the image pull Secrets.</param>
/// <param name="Volumes">The extra volumes of the pod.</param>
/// <param name="VolumeMounts">Where the extra volumes are mounted in the container.</param>
/// <param name="InitContainers">The containers that run to completion before the container starts.</param>
/// <param name="NodeSelector">The node labels the pod must match.</param>
public record PodShape(
    string? LivenessPath = null,
    string? StartupPath = null,
    IReadOnlyList<string>? ImagePullSecrets = null,
    IReadOnlyList<V1Volume>? Volumes = null,
    IReadOnlyList<V1VolumeMount>? VolumeMounts = null,
    IReadOnlyList<V1Container>? InitContainers = null,
    IReadOnlyDictionary<string, string>? NodeSelector = null);
