// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// The AuthProxy that fronts an application.
/// </summary>
public class AuthProxySpec
{
    /// <summary>
    /// Gets or sets whether AuthProxy runs. On when the application has authentication.
    /// </summary>
    [Description("Whether AuthProxy runs. On when the application has authentication.")]
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the AuthProxy image.
    /// </summary>
    [Description("The AuthProxy image.")]
    public string Image { get; set; } = "cratis/authproxy:latest";

    /// <summary>
    /// Gets or sets the port AuthProxy listens on.
    /// </summary>
    [Description("The port AuthProxy listens on.")]
    public int Port { get; set; } = 8080;

    /// <summary>
    /// Gets or sets the number of replicas.
    /// </summary>
    [Description("The number of replicas.")]
    public int Replicas { get; set; } = 1;

    /// <summary>
    /// Gets or sets the identity providers. The values each needs come from the environment.
    /// </summary>
    [Description("The identity providers. The values each needs come from the environment.")]
    public IList<AuthProxyProvider> Providers { get; set; } = [];
}
