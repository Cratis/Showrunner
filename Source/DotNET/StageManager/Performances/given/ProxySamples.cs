// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.StageManager.Performances.given;

/// <summary>
/// Performances that use the optional AuthProxy and Ante settings: a gateway in front of a console, and an application with a fully configured proxy.
/// </summary>
public static class ProxySamples
{
    public const string UpstreamUrl = "http://console-gateway.kubernetes-dashboard.svc.cluster.local/";
    public const string GitHubClientSecret = "fake-console-client-secret-0b6e";

    /// <summary>
    /// A gateway: AuthProxy in front of a Service, GitHub sign-in from a Secret, restricted to a team.
    /// </summary>
    /// <param name="thirdParty">Whether the upstream is not a Cratis service.</param>
    /// <returns>The Performance.</returns>
    public static Performance Gateway(bool thirdParty = true) => new()
    {
        Metadata = new() { Name = "console", NamespaceProperty = "apps", Uid = "uid-console", Generation = 1 },
        Spec = new()
        {
            AuthProxy = new()
            {
                Enabled = true,
                Providers =
                [
                    new()
                    {
                        Kind = AuthProxyProviderKind.GitHub,
                        Identity = "GitHub",
                        CredentialsSecretRef = new() { Name = "console-github" },
                        Scopes = ["read:user", "user:email", "read:org"]
                    }
                ],
                Upstream = new() { Url = UpstreamUrl, ThirdParty = thirdParty },
                Tenancy = new() { TenantId = "Default" },
                RequiredClaims =
                [
                    new() { Claim = "urn:github:organization", AnyOf = ["acme"] },
                    new() { Claim = "urn:github:team", AnyOf = ["acme/staff", "acme/ops"] }
                ],
                Pages = new() { ConfigMapRef = new() { Name = "console-pages" } },
                DataProtection = new() { ClaimName = "console-keys" },
                Resources = new() { Requests = { ["cpu"] = "100m", ["memory"] = "128Mi" } }
            },
            Ingress = new()
            {
                Enabled = true,
                ClassName = "nginx",
                Hosts = ["console.example.com"],
                TlsIssuer = new() { Name = "letsencrypt" },
                Annotations = { ["nginx.ingress.kubernetes.io/proxy-read-timeout"] = "3600" }
            }
        }
    };

    /// <summary>
    /// An application with onboarding whose proxy and Ante use every optional setting.
    /// </summary>
    /// <returns>The Performance.</returns>
    public static Performance ConfiguredApplication()
    {
        var performance = PerformanceSamples.Performance();
        performance.Spec.AuthProxy.Tenancy = new() { TenantsEndpoint = "http://crm-app/api/internal/tenants/selectable" };
        performance.Spec.AuthProxy.Pages = new() { ConfigMapRef = new() { Name = "crm-pages" } };
        performance.Spec.AuthProxy.Providers[1].Scopes = ["read:user", "user:email"];
        performance.Spec.Onboarding.RegistrationReview = true;
        performance.Spec.Onboarding.ReservedOrganizationNames = ["admin", "api"];
        performance.Spec.Onboarding.Branding = new()
        {
            LogoUrl = "/brand/logo.svg",
            CustomCssUrl = "/brand/lobby.css",
            PageTitle = "Crm",
            SplashMessage = "Loading Crm",
            Assets = new() { ConfigMapRef = new() { Name = "crm-brand" }, Files = ["logo.svg", "lobby.css"] }
        };
        return performance;
    }

    /// <summary>
    /// The environment of a gateway that takes its credentials from a Secret: it has none.
    /// </summary>
    /// <returns>The environment.</returns>
    public static ResolvedEnvironment NoEnvironment() => new(string.Empty, new Dictionary<string, string>(StringComparer.Ordinal));

    /// <summary>
    /// The environment the preparation hands the contracts once it has read the credentials Secret.
    /// </summary>
    /// <returns>The environment.</returns>
    public static ResolvedEnvironment CredentialsEnvironment() => new(string.Empty, new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["authentication.providers.GitHub.clientId"] = "fake-console-client-id",
        ["authentication.providers.GitHub.clientSecret"] = GitHubClientSecret
    });
}
#endif
