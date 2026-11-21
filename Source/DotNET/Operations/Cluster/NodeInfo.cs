// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// What the watchdog reads of a node.
/// </summary>
/// <param name="Name">The node name.</param>
/// <param name="Conditions">Its conditions.</param>
public sealed record NodeInfo(string Name, IReadOnlyList<NodeCondition> Conditions)
{
    /// <summary>
    /// The label that marks a node as carrying batch (CI runner) workloads.
    /// </summary>
    public const string WorkloadLabel = "workload";

    /// <summary>
    /// The value of <see cref="WorkloadLabel"/> that marks a batch node.
    /// </summary>
    public const string BatchWorkload = "batch";

    /// <summary>
    /// Gets the node labels.
    /// </summary>
    public IReadOnlyDictionary<string, string> Labels { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets when the node joined the cluster, if known.
    /// </summary>
    public DateTimeOffset? Created { get; init; }

    /// <summary>
    /// Gets a value indicating whether the node is being deleted.
    /// </summary>
    public bool Deleting { get; init; }

    /// <summary>
    /// Gets the keys of the node's taints.
    /// </summary>
    public IReadOnlyList<string> TaintKeys { get; init; } = [];

    /// <summary>
    /// Gets a value indicating whether the node carries batch workloads.
    /// </summary>
    public bool IsBatch => Labels.TryGetValue(WorkloadLabel, out var workload) && workload == BatchWorkload;

    /// <summary>
    /// Gets a value indicating whether the node is on its way out or has only just joined, so it is expected to have no summary and no metrics.
    /// A batch node takes longer to settle: a new one is pulling images and starting several builds at once.
    /// </summary>
    /// <param name="now">The current time.</param>
    /// <returns>True when the node is changing.</returns>
    public bool IsChanging(DateTimeOffset now)
    {
        if (Deleting || TaintKeys.Any(key => string.Equals(key, "ToBeDeletedByClusterAutoscaler", StringComparison.Ordinal) || string.Equals(key, "DeletionCandidateOfClusterAutoscaler", StringComparison.Ordinal)))
        {
            return true;
        }

        return Created is { } created && now - created < (IsBatch ? TimeSpan.FromMinutes(30) : TimeSpan.FromMinutes(10));
    }
}
