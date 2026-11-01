// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// A folder of files made from one or more Secrets and mounted read-only. This is the only way a resolved value reaches a container.
/// </summary>
/// <param name="VolumeName">The name of the volume.</param>
/// <param name="MountPath">Where the folder is mounted.</param>
/// <param name="Sources">The Secrets that make up the folder.</param>
public record SecretMount(string VolumeName, string MountPath, IReadOnlyList<SecretSource> Sources);
