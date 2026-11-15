// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Where a volume of the application's pod is mounted in a container.
/// </summary>
public class ApplicationVolumeMount
{
    /// <summary>
    /// Gets or sets the name of the volume.
    /// </summary>
    [Description("The name of a volume of the application.")]
    public string Volume { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the absolute path the volume is mounted at.
    /// </summary>
    [Description("The absolute path the volume is mounted at, such as /var/lib/trust.")]
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the mount is read-only.
    /// </summary>
    [Description("Mounts the volume read-only.")]
    public bool ReadOnly { get; set; }
}
