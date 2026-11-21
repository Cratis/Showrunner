// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// A ConfigMap mounted read-only, as a folder or, when files are named, as one file per key.
/// </summary>
/// <param name="VolumeName">The name of the volume.</param>
/// <param name="MountPath">Where the folder is mounted.</param>
/// <param name="ConfigMapName">The name of the ConfigMap.</param>
/// <param name="Files">The keys to mount each as its own file in the folder, or null to mount the whole ConfigMap as the folder.</param>
public record ConfigMapMount(string VolumeName, string MountPath, string ConfigMapName, IReadOnlyList<string>? Files = null);
