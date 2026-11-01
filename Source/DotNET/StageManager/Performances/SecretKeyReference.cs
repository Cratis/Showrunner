// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// A reference to one key of a Secret.
/// </summary>
public class SecretKeyReference
{
    /// <summary>
    /// Gets or sets the name of the Secret, in the namespace of the Performance.
    /// </summary>
    [Required]
    [Description("The name of the Secret, in the namespace of the Performance.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the key within the Secret.
    /// </summary>
    [Description("The key within the Secret. Defaults to connectionString for a connection Secret.")]
    public string? Key { get; set; }
}
