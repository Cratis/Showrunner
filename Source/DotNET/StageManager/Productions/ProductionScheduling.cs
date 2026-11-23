// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// Where the pods of a <see cref="Production"/> may run, for a cluster that dedicates nodes to it.
/// </summary>
public class ProductionScheduling
{
    /// <summary>
    /// Gets or sets the node labels every pod of the Production must match.
    /// </summary>
    [Description("The node labels Chronicle, a Managed Vault, a Managed graph and the members of a Managed database must match. database.nodeSelector and graph.nodeSelector win for their own pods.")]
    public IDictionary<string, string> NodeSelector { get; set; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets the taints the pods of the Production tolerate.
    /// </summary>
    [Description("The taints the same pods tolerate.")]
    public IList<ProductionToleration> Tolerations { get; set; } = [];

    /// <summary>
    /// Gets the node selector of a pod, with the component's own labels taking precedence.
    /// </summary>
    /// <param name="component">The node labels the component names itself, if any.</param>
    /// <returns>The node selector, or null when there is none.</returns>
    public IDictionary<string, string>? NodeSelectorFor(IDictionary<string, string>? component = null)
    {
        var merged = new Dictionary<string, string>(NodeSelector, StringComparer.Ordinal);
        foreach (var (key, value) in component ?? new Dictionary<string, string>())
        {
            merged[key] = value;
        }

        return merged.Count == 0 ? null : merged;
    }

    /// <summary>
    /// Gets the tolerations of a pod.
    /// </summary>
    /// <returns>The tolerations, or null when there are none.</returns>
    public IList<V1Toleration>? TolerationsForPod() =>
        Tolerations.Count == 0
            ? null
            : [.. Tolerations.Select(toleration => new V1Toleration
            {
                Key = string.IsNullOrEmpty(toleration.Key) ? null : toleration.Key,
                OperatorProperty = toleration.Operator,
                Value = string.IsNullOrEmpty(toleration.Value) ? null : toleration.Value,
                Effect = string.IsNullOrEmpty(toleration.Effect) ? null : toleration.Effect
            })];
}
