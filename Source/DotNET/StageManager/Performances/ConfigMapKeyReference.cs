// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// A reference to one key of a ConfigMap.
/// </summary>
public class ConfigMapKeyReference
{
    /// <summary>
    /// Gets or sets the name of the ConfigMap, in the namespace of the Performance.
    /// </summary>
    [Required]
    [Description("The name of the ConfigMap, in the namespace of the Performance.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the key within the ConfigMap.
    /// </summary>
    [Description("The key within the ConfigMap. For an environment file it defaults to <environment name>.yaml.")]
    public string? Key { get; set; }
}
