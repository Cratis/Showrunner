// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Performances;

/// <summary>
/// The AuthProxy that fronts the application. Declaring it turns AuthProxy on; leave <see cref="PerformanceArgs.AuthProxy"/> out and none runs.
/// </summary>
public sealed class AuthProxyArgs
{
    /// <summary>
    /// Gets the AuthProxy image, or null for the operator's default.
    /// </summary>
    public string? Image { get; init; }

    /// <summary>
    /// Gets the port AuthProxy listens on, or null for the operator's default.
    /// </summary>
    public int? Port { get; init; }

    /// <summary>
    /// Gets the number of replicas, or null for the operator's default.
    /// </summary>
    public int? Replicas { get; init; }

    /// <summary>
    /// Gets the identity providers.
    /// </summary>
    public IList<AuthProxyProviderArgs> Providers { get; init; } = [];

    /// <summary>
    /// Gets how the tenant of a request is resolved, or null to leave it to AuthProxy's default.
    /// </summary>
    public AuthProxyTenancyArgs? Tenancy { get; init; }

    /// <summary>
    /// Gets the claims the caller must carry. All of them are required.
    /// </summary>
    public IList<RequiredClaimArgs> RequiredClaims { get; init; } = [];

    /// <summary>
    /// Gets the custom pages, or null to keep AuthProxy's built-in pages.
    /// </summary>
    public AuthProxyPagesArgs? Pages { get; init; }

    /// <summary>
    /// Gets where the data protection key ring lives, or null to keep it in the pod.
    /// </summary>
    public AuthProxyDataProtectionArgs? DataProtection { get; init; }

    /// <summary>
    /// Gets the addresses or CIDR ranges of the peers whose forwarded headers AuthProxy believes: the ingress controller or load balancer
    /// in front of it. Without one it ignores them, and builds a sign-in callback with the http scheme the ingress terminated.
    /// </summary>
    public IList<string> TrustedProxies { get; init; } = [];

    /// <summary>
    /// Gets the resources of the AuthProxy container, or null to set none.
    /// </summary>
    public ApplicationResourcesArgs? Resources { get; init; }

    /// <summary>
    /// Gets the existing Service AuthProxy fronts instead of an application, which makes the Performance a gateway.
    /// </summary>
    public AuthProxyUpstreamArgs? Upstream { get; init; }

    /// <summary>
    /// Builds the spec of AuthProxy, declaring what it refers to.
    /// </summary>
    /// <param name="context">The context of the Performance.</param>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec(PerformanceContext context) =>
        SpecExtensions.NewSpec()
            .Put("enabled", true)
            .Put("image", Image)
            .Put("port", Port)
            .Put("replicas", Replicas)
            .PutList("providers", Providers.Select(provider => provider.ToSpec(context)))
            .PutObject("tenancy", Tenancy?.ToSpec() ?? SpecExtensions.NewSpec())
            .PutList("trustedProxies", TrustedProxies)
            .PutList("requiredClaims", RequiredClaims.Select(claim => claim.ToSpec()))
            .PutObject("pages", Pages?.ToSpec(context) ?? SpecExtensions.NewSpec())
            .PutObject("dataProtection", DataProtection?.ToSpec(context) ?? SpecExtensions.NewSpec())
            .PutObject("resources", Resources?.ToSpec() ?? SpecExtensions.NewSpec())
            .PutObject("upstream", Upstream?.ToSpec() ?? SpecExtensions.NewSpec());
}
