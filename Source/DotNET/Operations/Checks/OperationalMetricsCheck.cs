// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks;

/// <summary>
/// Reads the small platform vectors of Prometheus: node root filesystem usage, and whether the node, kube-state-metrics and
/// collector scrape targets are up and present. A subject that is missing is a finding, and so is a source that cannot be read:
/// an unreadable source never grants health. Nodes that are leaving or have just joined, and nodes no longer in the cluster, are not blamed.
/// </summary>
/// <param name="cluster">The cluster reader.</param>
/// <param name="prometheus">The Prometheus reader.</param>
/// <param name="options">The options.</param>
public sealed class OperationalMetricsCheck(IClusterReader cluster, IPrometheusReader prometheus, OperationalMetricsOptions options) : IWatchdogCheck
{
    /// <summary>
    /// The name of the check in configuration.
    /// </summary>
    public const string CheckName = "metrics";

    const string RootUsage = "root-usage";
    const string NodeTargets = "node-targets";
    const string ServiceTargets = "service-targets";
    const string CollectorTargets = "collector-targets";

    static readonly TimeSpan _budget = TimeSpan.FromSeconds(20);
    static readonly TimeSpan _requestTimeout = TimeSpan.FromSeconds(2);
    static readonly string[] _nodeJobs = ["kubernetes-nodes", "kubernetes-nodes-cadvisor"];

    /// <inheritdoc/>
    public string Name => CheckName;

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Finding>> Collect(CheckContext context, CancellationToken cancellationToken)
    {
        var findings = new List<Finding>();
        IReadOnlyList<NodeInfo> nodes = [];
        try
        {
            nodes = await cluster.ListNodes(cancellationToken);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            findings.Add(new Finding("nodes:api", $"could not read nodes: {Redaction.Summarize(error.Message)}; inspect watchdog API access"));
        }

        var names = nodes.Select(node => node.Name).ToHashSet(StringComparer.Ordinal);
        var changing = nodes.Where(node => node.IsChanging(context.Now)).Select(node => node.Name).ToHashSet(StringComparer.Ordinal);
        var deadline = Earliest(context.TimeProvider.GetUtcNow() + _budget, context.CollectionDeadline);
        var ns = options.PlatformNamespace;
        var queries = new (string Check, string Expression)[]
        {
            (RootUsage, $"1 - node_filesystem_avail_bytes{{namespace=\"{ns}\",mountpoint=\"/\"}} / node_filesystem_size_bytes{{namespace=\"{ns}\",mountpoint=\"/\"}}"),
            (NodeTargets, "up{job=~\"kubernetes-nodes|kubernetes-nodes-cadvisor\"}"),
            (ServiceTargets, $"up{{namespace=\"{ns}\",job=\"kubernetes-service-endpoints\",service=~\".*-kube-state-metrics\"}}"),
            (CollectorTargets, $"up{{namespace=\"{ns}\",job=\"kubernetes-pods\",app=\"otel-collector\"}}")
        };

        foreach (var (check, expression) in queries)
        {
            try
            {
                var remaining = deadline - context.TimeProvider.GetUtcNow();
                if (remaining <= TimeSpan.Zero)
                {
                    throw new TimeoutException("telemetry query exhausted its check budget or alert-delivery reserve");
                }

                var samples = await prometheus.Query(expression, remaining < _requestTimeout ? remaining : _requestTimeout, cancellationToken);
                findings.AddRange(Evaluate(check, samples, names, changing));
            }
            catch (Exception error) when (error is not OperationCanceledException)
            {
                // Never grant health to an unreadable source.
                findings.Add(new Finding($"metrics:{check}:api", $"could not read {check}: {Redaction.Summarize(error.Message)}; inspect Prometheus and watchdog query access"));
            }
        }

        return findings;
    }

    static DateTimeOffset Earliest(DateTimeOffset first, DateTimeOffset second) => first < second ? first : second;

    List<Finding> Evaluate(string check, IReadOnlyList<PrometheusSample> samples, HashSet<string> nodes, HashSet<string> changing)
    {
        var findings = new List<Finding>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var sample in samples)
        {
            // A node the autoscaler removed leaves a down series behind until Prometheus drops the target. An unreadable
            // node list never silences a target: only a read list can say the node is gone.
            var nodeLabel = check switch
            {
                RootUsage => sample.Label("node"),
                NodeTargets => sample.Label("instance"),
                _ => null
            };
            if (nodes.Count > 0 && nodeLabel is not null && !nodes.Contains(nodeLabel))
            {
                continue;
            }

            if (check == RootUsage)
            {
                var identity = sample.Label("node") ?? throw new InvalidOperationException("query returned an unreadable sample");
                seen.Add(identity);
                if (sample.Value > options.RootUsageThreshold && !changing.Contains(identity))
                {
                    findings.Add(new Finding(
                        $"node:{identity}:{check}",
                        $"node {identity} root filesystem {(sample.Value * 100).ToString("F1", CultureInfo.InvariantCulture)}% used; inspect image cache and disk growth; {options.Advice}"));
                }

                continue;
            }

            var job = sample.Label("job") ?? string.Empty;
            var instance = sample.Label("instance") ?? string.Empty;
            var subject = sample.Label("service") ?? (check == CollectorTargets ? "otel-collector" : instance);
            seen.Add(check == ServiceTargets ? subject : $"{job}\u0000{subject}");
            if (sample.Value == 0 && !changing.Contains(instance))
            {
                findings.Add(new Finding(
                    $"telemetry:target-down:{job}/{instance}",
                    $"platform scrape target {subject} ({job}/{instance}) is down; inspect target health and scrape access"));
            }
        }

        foreach (var subject in Missing(check, samples, nodes, changing, seen))
        {
            findings.Add(new Finding(
                $"metrics:{check}:missing:{subject}",
                $"{subject}: {check} data missing; inspect exporter, Service annotations and Prometheus scrape health"));
        }

        return findings;
    }

    IEnumerable<string> Missing(string check, IReadOnlyList<PrometheusSample> samples, HashSet<string> nodes, HashSet<string> changing, HashSet<string> seen) => check switch
    {
        RootUsage => nodes.Where(node => !changing.Contains(node) && !seen.Contains(node)).Order(StringComparer.Ordinal),
        NodeTargets => _nodeJobs
            .SelectMany(job => nodes.Where(node => !changing.Contains(node)).Select(node => (Job: job, Node: node)))
            .Where(pair => !seen.Contains($"{pair.Job}\u0000{pair.Node}"))
            .Select(pair => $"('{pair.Job}', '{pair.Node}')")
            .Order(StringComparer.Ordinal),
        ServiceTargets => seen.Any(name => name.EndsWith("-kube-state-metrics", StringComparison.Ordinal)) ? [] : ["kube-state-metrics"],
        _ => samples.Count == 0 ? ["otel-collector"] : []
    };
}
