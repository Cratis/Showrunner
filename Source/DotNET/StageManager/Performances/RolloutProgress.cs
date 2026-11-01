// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// A rollout that is under way: which release is being waited for and since when.
/// </summary>
public class RolloutProgress
{
    /// <summary>
    /// Gets or sets the application version being rolled out.
    /// </summary>
    [Description("The application version being rolled out.")]
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the runtime image being rolled out.
    /// </summary>
    [Description("The Stage runtime image being rolled out.")]
    public string Image { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the package ConfigMap being rolled out.
    /// </summary>
    [Description("The package ConfigMap being rolled out.")]
    public string PackageConfigMap { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the rollout started.
    /// </summary>
    [Description("When the rollout started.")]
    public DateTimeOffset StartedAt { get; set; }
}
