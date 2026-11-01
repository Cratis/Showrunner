// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// The usage of one container.
/// </summary>
public class ContainerMetrics
{
    /// <summary>
    /// Gets or sets the name of the container.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the usage by resource name: <c>cpu</c> and <c>memory</c>.
    /// </summary>
    [JsonPropertyName("usage")]
    public IDictionary<string, ResourceQuantity> Usage { get; set; } = new Dictionary<string, ResourceQuantity>(StringComparer.Ordinal);
}
