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
/// <param name="StagePackage">The ConfigMap the package archive volume of the pod template reads from, or null when there is none.</param>
public record ObservedDeployment(
    string? StageImage,
    int Replicas,
    int ReadyReplicas,
    int UpdatedReplicas,
    long Generation,
    long ObservedGeneration,
    bool ProgressDeadlineExceeded,
    string? StagePackage = null)
{
    /// <summary>Decides whether the Deployment runs only the given image and package and every pod is ready.</summary>
    /// <param name="image">The image.</param>
    /// <param name="package">The package ConfigMap, or null when the image carries the package.</param>
    /// <returns>True when the rollout has completed.</returns>
    public bool IsRolledOutAt(string image, string? package = null) =>
        StageImage == image
        && StagePackage == package
        && ObservedGeneration >= Generation
        && Replicas == 1
        && UpdatedReplicas == 1
        && ReadyReplicas == 1;
}
