// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Performances;

/// <summary>
/// The kind of a volume of an application's pod.
/// </summary>
public enum VolumeKind
{
    /// <summary>
    /// A scratch directory that lives as long as the pod. Init containers and the application share it.
    /// </summary>
    EmptyDir = 0,

    /// <summary>
    /// An existing PersistentVolumeClaim in the namespace of the Performance. Stage Manager never creates or deletes the claim.
    /// </summary>
    PersistentVolumeClaim = 1
}
