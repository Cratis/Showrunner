// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Checks;

/// <summary>
/// The options of <see cref="ReadinessCheck"/>.
/// </summary>
public sealed record ReadinessCheckOptions
{
    /// <summary>
    /// Gets the namespace of the pods to probe.
    /// </summary>
    public required string Namespace { get; init; }

    /// <summary>
    /// Gets the label selector that finds the pods, for example <c>app=core</c>.
    /// </summary>
    public required string LabelSelector { get; init; }

    /// <summary>
    /// Gets the name of the container whose state decides whether a pod is probed; blank means any state.
    /// </summary>
    public string? ContainerName { get; init; }

    /// <summary>
    /// Gets the port the readiness endpoint listens on.
    /// </summary>
    public int Port { get; init; } = 8080;

    /// <summary>
    /// Gets the path of the readiness endpoint.
    /// </summary>
    public string Path { get; init; } = "/health/ready";

    /// <summary>
    /// Gets how long a probe may take before the pod counts as hung.
    /// </summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(20);

    /// <summary>
    /// Gets what to tell the reader to do about a pod that is listening but not answering.
    /// </summary>
    public string HangAdvice { get; init; } = "inspect its logs, then delete the pod to recover it";
}
