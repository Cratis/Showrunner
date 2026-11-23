// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Productions;

/// <summary>
/// Defines where the pods of a Production may run, for a cluster that dedicates nodes to it.
/// </summary>
public sealed class ProductionSchedulingArgs
{
    /// <summary>
    /// Gets the node labels Chronicle, a managed Vault, a managed graph and the members of a managed database must match.
    /// </summary>
    public IDictionary<string, string> NodeSelector { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Gets the taints the same pods tolerate.
    /// </summary>
    public IList<ProductionTolerationArgs> Tolerations { get; init; } = [];

    /// <summary>
    /// Builds the scheduling as the operator's spec writes it.
    /// </summary>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec() =>
        SpecExtensions.NewSpec()
            .PutMap("nodeSelector", NodeSelector.Select(pair => new KeyValuePair<string, object>(pair.Key, pair.Value)))
            .PutList("tolerations", Tolerations.Select(toleration => toleration.ToSpec()));
}
