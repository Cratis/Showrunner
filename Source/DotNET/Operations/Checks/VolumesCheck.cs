// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks;

/// <summary>
/// Finds persistent volume claims that are nearly full, and bound claims that are mounted by a settled pod but report no usage.
/// Usage comes from each node's kubelet; a node that cannot be read is a finding of its own and its claims are not blamed.
/// </summary>
/// <param name="reader">The cluster reader.</param>
/// <param name="options">The options.</param>
public sealed class VolumesCheck(IClusterReader reader, VolumesCheckOptions options) : IWatchdogCheck
{
    /// <summary>
    /// The name of the check in configuration.
    /// </summary>
    public const string CheckName = "volumes";

    /// <summary>
    /// The kubelet reports volume usage a few minutes after a container starts.
    /// </summary>
    static readonly TimeSpan _statsGrace = TimeSpan.FromMinutes(10);

    /// <summary>
    /// A summary is an API-server proxy hop to a busy kubelet; one read in a while stalls, so it is retried once, longer.
    /// </summary>
    static readonly TimeSpan[] _summaryTimeouts = [TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10)];

    /// <inheritdoc/>
    public string Name => CheckName;

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Finding>> Collect(CheckContext context, CancellationToken cancellationToken)
    {
        var findings = new List<Finding>();
        var nodes = await Read(() => reader.ListNodes(cancellationToken), "nodes:api", "could not read nodes", findings);
        var pods = await Read(() => reader.ListPods(null, null, cancellationToken), "pods:api", "could not read cluster pod status", findings);

        var usage = new Dictionary<string, VolumeUsage>(StringComparer.Ordinal);
        var summarized = new HashSet<string>(StringComparer.Ordinal);
        foreach (var node in nodes)
        {
            var read = await ReadSummary(node.Name, cancellationToken);
            if (read.Usage is null)
            {
                findings.Add(new Finding(
                    $"node:{node.Name}:summary",
                    $"could not read kubelet summary for node {node.Name}: {Redaction.Summarize(read.Error)}; inspect kubelet health and watchdog access",
                    AlertSeverity.Warning));
                continue;
            }

            summarized.Add(node.Name);
            foreach (var volume in read.Usage.Where(volume => volume.CapacityBytes > 0))
            {
                var identity = $"{volume.Namespace}/{volume.ClaimName}";
                usage[identity] = volume;
                var percent = volume.UsedBytes * 100.0 / volume.CapacityBytes;
                if (percent > Math.Min(85, options.ThresholdPercent))
                {
                    findings.Add(new Finding(
                        $"pvc:{identity}",
                        $"volume {identity} at {percent:F1}% ({volume.UsedBytes >> 30}Gi of {volume.CapacityBytes >> 30}Gi); {options.Advice}",
                        percent > 95 ? AlertSeverity.Critical : AlertSeverity.Warning));
                }
            }
        }

        var notReady = nodes.Where(node => node.Conditions.Any(condition => condition.Type == "Ready" && condition.Status != "True")).Select(node => node.Name).ToHashSet(StringComparer.Ordinal);
        var mounted = pods
            .Where(pod => pod.Phase == "Running" && IsSettled(pod, context.Now) && pod.NodeName is not null && summarized.Contains(pod.NodeName) && !notReady.Contains(pod.NodeName))
            .SelectMany(pod => (pod.Claims ?? []).Select(claim => $"{pod.Namespace}/{claim}"))
            .ToHashSet(StringComparer.Ordinal);

        // With no readable node list or summaries there is nothing to compare against; those are reported above.
        if (mounted.Count > 0)
        {
            await AddMissingUsage(mounted, usage, findings, cancellationToken);
        }

        return findings;
    }

    static bool IsSettled(PodInfo pod, DateTimeOffset now) =>
        pod.Started is { } started && pod.Ready && now - started >= _statsGrace;

    async Task AddMissingUsage(HashSet<string> mounted, Dictionary<string, VolumeUsage> usage, List<Finding> findings, CancellationToken cancellationToken)
    {
        try
        {
            foreach (var claim in await reader.ListClaims(cancellationToken))
            {
                var identity = $"{claim.Namespace}/{claim.Name}";
                if (claim.Phase == "Bound" && mounted.Contains(identity) && !usage.ContainsKey(identity))
                {
                    findings.Add(new Finding(
                        $"pvc:{identity}:usage-missing",
                        $"bound volume {identity} has no usage data; inspect its mount and kubelet volume stats; {options.Advice}",
                        AlertSeverity.Warning));
                }
            }
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            findings.Add(new Finding("pvc:api", $"could not list PVCs: {Redaction.Summarize(error.Message)}; inspect watchdog API access", AlertSeverity.Warning));
        }
    }

    async Task<(IReadOnlyList<VolumeUsage>? Usage, string Error)> ReadSummary(string node, CancellationToken cancellationToken)
    {
        var error = string.Empty;
        foreach (var timeout in _summaryTimeouts)
        {
            try
            {
                return (await reader.GetVolumeUsage(node, timeout, cancellationToken), string.Empty);
            }
            catch (Exception exception) when (exception is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
            {
                error = exception.Message;
            }
        }

        return (null, error);
    }

    async Task<IReadOnlyList<T>> Read<T>(Func<Task<IReadOnlyList<T>>> read, string key, string what, List<Finding> findings)
    {
        try
        {
            return await read();
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            findings.Add(new Finding(key, $"{what}: {Redaction.Summarize(error.Message)}; inspect watchdog API access", AlertSeverity.Warning));
            return [];
        }
    }
}
