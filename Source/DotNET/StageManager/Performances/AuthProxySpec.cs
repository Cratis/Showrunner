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

    /// <summary>
    /// Gets or sets how the tenant of a request is resolved. Omit to leave it to AuthProxy's default.
    /// </summary>
    [Description("How the tenant of a request is resolved. Omit to leave it to AuthProxy's default.")]
    public AuthProxyTenancySpec? Tenancy { get; set; }

    /// <summary>
    /// Gets or sets the claims the caller must carry. All of them are required.
    /// </summary>
    [Description("The claims the caller must carry before anything is forwarded. Every entry must be satisfied; the values of one entry are alternatives.")]
    public IList<AuthProxyRequiredClaim> RequiredClaims { get; set; } = [];

    /// <summary>
    /// Gets or sets the custom pages. Omit to keep AuthProxy's built-in pages.
    /// </summary>
    [Description("The custom pages AuthProxy shows. Omit to keep its built-in pages.")]
    public AuthProxyPagesSpec? Pages { get; set; }

    /// <summary>
    /// Gets or sets where the data protection key ring lives. Omit to keep it in the pod.
    /// </summary>
    [Description("Where the data protection key ring lives, so sessions survive a restart. Omit to keep it in the pod, where a restart signs everyone out.")]
    public AuthProxyDataProtectionSpec? DataProtection { get; set; }

    /// <summary>
    /// Gets or sets the peers whose forwarded headers AuthProxy believes.
    /// </summary>
    [Description("The addresses or CIDR ranges of the peers AuthProxy believes when they say which client and scheme a request came from through X-Forwarded-For and X-Forwarded-Proto: the ingress controller or load balancer in front of it, such as 10.0.0.0/8. Without one, forwarded headers are ignored, and a sign-in callback is built with the http scheme the ingress terminated.")]
    public IList<string> TrustedProxies { get; set; } = [];

    /// <summary>
    /// Gets or sets the resources of the AuthProxy container.
    /// </summary>
    [Description("The resource requests and limits of the AuthProxy container. Omit to set none.")]
    public ResourcesSpec? Resources { get; set; }

    /// <summary>
    /// Gets or sets the existing Service AuthProxy fronts instead of an application. Naming one makes the Performance a gateway.
    /// </summary>
    [Description("An existing Service AuthProxy fronts instead of an application. Naming one makes the Performance a gateway: only AuthProxy runs, and application, onboarding and chronicle are not used.")]
    public AuthProxyUpstreamSpec? Upstream { get; set; }
}
