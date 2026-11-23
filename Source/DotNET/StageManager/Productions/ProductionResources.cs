// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// The compute resources of the Chronicle container of a <see cref="Production"/>.
/// </summary>
public class ProductionResources
{
    /// <summary>
    /// Gets or sets what the scheduler reserves, such as cpu: 500m and memory: 1Gi.
    /// </summary>
    [Description("What the scheduler reserves, by resource name, such as cpu: 500m and memory: 1Gi.")]
    public IDictionary<string, string> Requests { get; set; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets the most the container may use, by resource name.
    /// </summary>
    [Description("The most the container may use, by resource name.")]
    public IDictionary<string, string> Limits { get; set; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Gets the requirements of a container.
    /// </summary>
    /// <returns>The requirements, or null when none are set.</returns>
    public V1ResourceRequirements? ToRequirements() =>
        Requests.Count == 0 && Limits.Count == 0
            ? null
            : new()
            {
                Requests = Requests.Count == 0 ? null : Requests.ToDictionary(pair => pair.Key, pair => new ResourceQuantity(pair.Value), StringComparer.Ordinal),
                Limits = Limits.Count == 0 ? null : Limits.ToDictionary(pair => pair.Key, pair => new ResourceQuantity(pair.Value), StringComparer.Ordinal)
            };
}
