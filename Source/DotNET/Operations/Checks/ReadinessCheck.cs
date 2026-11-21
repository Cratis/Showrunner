// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Sockets;
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks;

/// <summary>
/// Asks every pod of a workload for its readiness endpoint directly by pod IP. The Service round-robins, so going through it
/// would only catch a bad replica part of the time. It catches a replica whose process stops answering; a hang confined to a
/// request path readiness does not run is not visible here. No identity is sent: the watchdog must not depend on a header only
/// the trusted proxies are meant to set.
/// </summary>
/// <param name="reader">The cluster reader.</param>
/// <param name="client">The HTTP client used to probe.</param>
/// <param name="options">The workload and endpoint.</param>
/// <param name="timeProvider">The clock, which bounds each probe.</param>
public sealed class ReadinessCheck(IClusterReader reader, HttpClient client, ReadinessCheckOptions options, TimeProvider timeProvider) : IWatchdogCheck
{
    /// <summary>
    /// The name of the check in configuration.
    /// </summary>
    public const string CheckName = "readiness";

    /// <inheritdoc/>
    public string Name => CheckName;

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Finding>> Collect(CheckContext context, CancellationToken cancellationToken)
    {
        IReadOnlyList<PodInfo> pods;
        try
        {
            pods = await reader.ListPods(options.Namespace, options.LabelSelector, cancellationToken);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            return [new Finding($"readiness:{options.Namespace}:list-pods", $"could not list pods in {options.Namespace} to check readiness: {Redaction.Summarize(error.Message)}", AlertSeverity.Warning)];
        }

        var findings = new List<Finding>();
        foreach (var pod in pods.Where(pod => pod.Phase == "Running" && pod.PodIp is not null && IsProbeable(pod)))
        {
            var identity = $"{pod.Namespace}/{pod.Name}";
            if (context.Remaining < options.Timeout)
            {
                findings.Add(new Finding($"watchdog:budget:readiness:{identity}", $"{identity} readiness not probed: collection budget exhausted", AlertSeverity.Warning));
                continue;
            }

            if (await Probe(pod, identity, cancellationToken) is { } finding)
            {
                findings.Add(finding);
            }
        }

        return findings;
    }

    /// <summary>
    /// A container that is crash-looping or still on its first start cycles through Running and is already reported under its own,
    /// less alarming finding. The wedge diagnosis is reserved for a container that is up and simply never answers.
    /// </summary>
    /// <param name="pod">The pod.</param>
    /// <returns>Whether the pod should be probed.</returns>
    bool IsProbeable(PodInfo pod)
    {
        var container = pod.Containers.FirstOrDefault(candidate => candidate.Name == options.ContainerName);
        return container is null || (container.Ready && container.RestartCount == 0);
    }

    async Task<Finding?> Probe(PodInfo pod, string identity, CancellationToken cancellationToken)
    {
        var url = new Uri($"http://{pod.PodIp}:{options.Port}{options.Path}");
        using var timeout = new CancellationTokenSource(options.Timeout, timeProvider);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
        try
        {
            using var response = await client.GetAsync(url, linked.Token);
            return response.IsSuccessStatusCode
                ? null
                : new Finding(
                    $"pod:{identity}:NotReady",
                    $"{identity} answered {options.Path} with HTTP {(int)response.StatusCode}; inspect its dependencies",
                    AlertSeverity.Critical);
        }
        catch (HttpRequestException error) when (error.InnerException is SocketException { SocketErrorCode: SocketError.ConnectionRefused })
        {
            // Nothing is listening: still starting or just crashed, a different and much less alarming situation than a wedge.
            return new Finding(
                $"pod:{identity}:ReadinessRefused",
                $"{identity} refused the readiness call - nothing is listening, likely still starting or just restarted rather than wedged",
                AlertSeverity.Warning);
        }
        catch (Exception error) when (!cancellationToken.IsCancellationRequested)
        {
            var reason = error is OperationCanceledException ? $"no answer within {(int)options.Timeout.TotalSeconds}s" : Redaction.Summarize(error.Message);
            return new Finding(
                $"pod:{identity}:ReadinessHang",
                $"{identity} did not answer {options.Path} within {(int)options.Timeout.TotalSeconds}s ({reason}) - the process is listening but not answering; {options.HangAdvice}",
                AlertSeverity.Critical);
        }
    }
}
