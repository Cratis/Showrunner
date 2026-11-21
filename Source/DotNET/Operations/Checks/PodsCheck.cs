// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks;

/// <summary>
/// Finds pods that are broken in a way self-healing does not fix: containers stuck waiting on a bad reason (crash loop, image
/// pull, configuration), pods evicted since the previous run, and pods that have been Pending for more than fifteen minutes.
/// </summary>
/// <param name="reader">The cluster reader.</param>
public sealed class PodsCheck(IClusterReader reader) : IWatchdogCheck
{
    /// <summary>
    /// The name of the check in configuration.
    /// </summary>
    public const string CheckName = "pods";

    static readonly HashSet<string> _badWaiting = new(StringComparer.Ordinal)
    {
        "CrashLoopBackOff",
        "ImagePullBackOff",
        "ErrImagePull",
        "CreateContainerConfigError",
        "CreateContainerError"
    };

    static readonly TimeSpan _pendingThreshold = TimeSpan.FromMinutes(15);

    /// <inheritdoc/>
    public string Name => CheckName;

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Finding>> Collect(CheckContext context, CancellationToken cancellationToken)
    {
        IReadOnlyList<PodInfo> pods;
        try
        {
            pods = await reader.ListPods(null, null, cancellationToken);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            return [new Finding("pods:api", $"could not read cluster pod status: {Redaction.Summarize(error.Message)}; inspect watchdog API access", AlertSeverity.Warning)];
        }

        var findings = new List<Finding>();
        foreach (var pod in pods)
        {
            var identity = $"{pod.Namespace}/{pod.Name}";
            findings.AddRange(pod.Containers
                .Where(container => container.WaitingReason is not null && _badWaiting.Contains(container.WaitingReason))
                .Select(container => new Finding(
                    $"pod:{identity}:{container.WaitingReason}",
                    $"{identity} container '{container.Name}': {container.WaitingReason} (restarts: {container.RestartCount})",
                    AlertSeverity.Critical)));

            if (pod.Reason == "Evicted" && EvictionFinding(pod, identity, context) is { } eviction)
            {
                findings.Add(eviction);
            }

            if (pod.Phase == "Pending" && PendingFinding(pod, identity, context) is { } pending)
            {
                findings.Add(pending);
            }
        }

        return findings;
    }

    static Finding? EvictionFinding(PodInfo pod, string identity, CheckContext context)
    {
        // Creation and start time are not eviction time: use the kubelet's disruption transition or the container termination.
        var evictedAt = pod.Containers.Select(container => container.TerminatedAt).Append(pod.DisruptedAt).Max();
        if (evictedAt is not null && evictedAt < context.WindowStarted)
        {
            return null;
        }

        var when = evictedAt is null ? " (eviction time unavailable)" : " in the last check window";
        return new Finding($"pod:{identity}:Evicted", $"{identity} was evicted{when}; inspect node pressure and workload requests before replacing it", AlertSeverity.Warning);
    }

    static Finding? PendingFinding(PodInfo pod, string identity, CheckContext context)
    {
        if (pod.Created is not { } created || context.Now - created <= _pendingThreshold)
        {
            return null;
        }

        // A pod stuck on ContainerCreating (pulling an image, mounting a volume) is a different situation from one that was
        // never scheduled; say which, instead of a bare Pending that reads the same either way.
        var reason = pod.Containers.Select(container => container.WaitingReason).FirstOrDefault(waiting => !string.IsNullOrEmpty(waiting));
        var suffix = reason is null ? string.Empty : $" ({reason})";
        return new Finding($"pod:{identity}:Pending", $"{identity} has been Pending{suffix} for {(int)(context.Now - created).TotalMinutes} minutes", AlertSeverity.Warning);
    }
}
