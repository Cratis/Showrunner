// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Performances;

/// <summary>
/// The HTTP paths Kubernetes checks the application on. A path left out is not checked over HTTP: readiness then checks the
/// port, and liveness and startup are not used.
/// </summary>
public sealed class ApplicationProbesArgs
{
    /// <summary>
    /// Gets the path that answers when the application is ready to serve.
    /// </summary>
    public string? ReadinessPath { get; init; }

    /// <summary>
    /// Gets the path that answers while the application is alive; the container is restarted when it stops answering.
    /// </summary>
    public string? LivenessPath { get; init; }

    /// <summary>
    /// Gets the path that answers once the application has started. Liveness and readiness wait for it.
    /// </summary>
    public string? StartupPath { get; init; }

    /// <summary>
    /// Builds the spec of the probes.
    /// </summary>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec() =>
        SpecExtensions.NewSpec()
            .Put("readinessPath", ReadinessPath)
            .Put("livenessPath", LivenessPath)
            .Put("startupPath", StartupPath);
}
