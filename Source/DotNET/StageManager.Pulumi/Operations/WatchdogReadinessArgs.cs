// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Operations;

/// <summary>
/// A workload whose pods the <see cref="Watchdog"/> asks for readiness directly by pod IP.
/// </summary>
public sealed class WatchdogReadinessArgs
{
    /// <summary>
    /// Gets the namespace of the pods.
    /// </summary>
    public required string Namespace { get; init; }

    /// <summary>
    /// Gets the label selector that finds the pods, for example <c>app=core</c>.
    /// </summary>
    public required string Selector { get; init; }

    /// <summary>
    /// Gets the name of the container whose state decides whether a pod is probed, or null to probe every running pod.
    /// </summary>
    public string? Container { get; init; }

    /// <summary>
    /// Gets the port of the readiness endpoint, or null for 8080.
    /// </summary>
    public int? Port { get; init; }

    /// <summary>
    /// Gets the path of the readiness endpoint, or null for <c>/health/ready</c>.
    /// </summary>
    public string? Path { get; init; }

    /// <summary>
    /// Gets what the alert tells the reader to do about a pod that listens but does not answer, or null for the default.
    /// </summary>
    public string? HangAdvice { get; init; }
}
