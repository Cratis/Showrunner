// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// One entry of the version history of a published application.
/// </summary>
public class RolloutRecord
{
    /// <summary>
    /// Gets or sets the application version.
    /// </summary>
    [Description("The application version.")]
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the runtime image.
    /// </summary>
    [Description("The Stage runtime image.")]
    public string Image { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the package ConfigMap.
    /// </summary>
    [Description("The name of the package ConfigMap.")]
    public string PackageConfigMap { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the outcome was decided.
    /// </summary>
    [Description("When the outcome was decided.")]
    public DateTimeOffset? RolledOutAt { get; set; }

    /// <summary>
    /// Gets or sets the outcome.
    /// </summary>
    [Description("RolledOut, RolledBack or Failed.")]
    public RolloutOutcome Outcome { get; set; }

    /// <summary>
    /// Gets or sets why, for a failure or a rollback.
    /// </summary>
    [Description("Why the version failed or was restored.")]
    public string Reason { get; set; } = string.Empty;
}
