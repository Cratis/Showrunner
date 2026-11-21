// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks;

/// <summary>
/// Finds nodes that are not Ready and nodes under memory, disk or PID pressure. A node that is not Ready can still pass a
/// plain TCP load-balancer check while failing every real request.
/// </summary>
/// <param name="reader">The cluster reader.</param>
public sealed class NodesCheck(IClusterReader reader) : IWatchdogCheck
{
    /// <summary>
    /// The name of the check in configuration.
    /// </summary>
    public const string CheckName = "nodes";

    static readonly string[] _pressures = ["MemoryPressure", "DiskPressure", "PIDPressure"];

    /// <inheritdoc/>
    public string Name => CheckName;

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Finding>> Collect(CheckContext context, CancellationToken cancellationToken)
    {
        IReadOnlyList<NodeInfo> nodes;
        try
        {
            nodes = await reader.ListNodes(cancellationToken);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            return [new Finding("nodes:api", $"could not read nodes: {Redaction.Summarize(error.Message)}; inspect watchdog API access", AlertSeverity.Warning)];
        }

        var findings = new List<Finding>();
        foreach (var node in nodes)
        {
            var ready = node.Conditions.FirstOrDefault(condition => condition.Type == "Ready");
            if (ready is not null && ready.Status != "True")
            {
                findings.Add(new Finding(
                    $"node:{node.Name}:NotReady",
                    $"node {node.Name} is not Ready (status: {ready.Status}, reason: {ready.Reason ?? "unknown"}) — a node in this state can still pass a plain TCP load-balancer health check while failing every real request",
                    AlertSeverity.Critical));
            }

            findings.AddRange(node.Conditions
                .Where(condition => _pressures.Contains(condition.Type) && condition.Status == "True")
                .Select(condition => new Finding(
                    $"node:{node.Name}:{condition.Type}",
                    $"node {node.Name} reports {condition.Type}={condition.Status}; inspect node resource use and eviction events; reduce load or restore capacity",
                    AlertSeverity.Warning)));
        }

        return findings;
    }
}
