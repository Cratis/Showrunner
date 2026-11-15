// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// The HTTP paths Kubernetes checks the application on. A path left empty is not checked over HTTP: readiness then checks the port, and liveness and startup are not used.
/// </summary>
public class ApplicationProbes
{
    /// <summary>
    /// Gets or sets the path that answers when the application is ready to serve.
    /// </summary>
    [Description("The HTTP path that answers when the application is ready to serve. Empty checks that the port accepts connections.")]
    public string ReadinessPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path that answers while the application is alive.
    /// </summary>
    [Description("The HTTP path that answers while the application is alive; the container is restarted when it stops answering. Empty uses no liveness check.")]
    public string LivenessPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path that answers once the application has started.
    /// </summary>
    [Description("The HTTP path that answers once the application has started. Liveness and readiness wait for it, for up to five minutes. Empty uses no startup check.")]
    public string StartupPath { get; set; } = string.Empty;
}
