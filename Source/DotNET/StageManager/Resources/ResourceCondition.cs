// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Resources;

/// <summary>
/// One observation about a resource, in the shape Kubernetes tooling expects of a condition.
/// </summary>
public class ResourceCondition
{
    /// <summary>
    /// Gets or sets what the condition is about, for example <c>AuthProxyReady</c>.
    /// </summary>
    [Required]
    [Description("What the condition is about, for example AuthProxyReady.")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the condition holds: <c>True</c>, <c>False</c> or <c>Unknown</c>.
    /// </summary>
    [Required]
    [Description("True, False or Unknown.")]
    public string Status { get; set; } = "Unknown";

    /// <summary>
    /// Gets or sets a short, machine-readable reason in CamelCase.
    /// </summary>
    [Description("A short, machine-readable reason in CamelCase.")]
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a human-readable message. It never contains a secret value.
    /// </summary>
    [Description("A human-readable message. It never contains a secret value.")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the condition last changed from one status to another.
    /// </summary>
    [Description("When the condition last changed status.")]
    public DateTimeOffset? LastTransitionTime { get; set; }
}
