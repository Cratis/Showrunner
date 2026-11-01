// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageOperator.Reconciling;

/// <summary>Represents the rollout progress of the Deployment.</summary>
/// <param name="StageImage">The image of the Stage container in the Deployment's pod template.</param>
/// <param name="Replicas">The total number of pods, old and new.</param>
/// <param name="ReadyReplicas">The number of ready pods.</param>
/// <param name="UpdatedReplicas">The number of pods on the current template.</param>
/// <param name="Generation">The generation of the Deployment.</param>
/// <param name="ObservedGeneration">The generation the Deployment controller has seen.</param>
/// <param name="ProgressDeadlineExceeded">Whether Kubernetes gave up on the rollout.</param>
public record ObservedDeployment(
    string? StageImage,
    int Replicas,
    int ReadyReplicas,
    int UpdatedReplicas,
    long Generation,
    long ObservedGeneration,
    bool ProgressDeadlineExceeded)
{
    /// <summary>Decides whether the Deployment runs only the given image and every pod is ready.</summary>
    /// <param name="image">The image.</param>
    /// <returns>True when the rollout has completed.</returns>
    public bool IsRolledOutAt(string image) =>
        StageImage == image
        && ObservedGeneration >= Generation
        && Replicas == 1
        && UpdatedReplicas == 1
        && ReadyReplicas == 1;
}
