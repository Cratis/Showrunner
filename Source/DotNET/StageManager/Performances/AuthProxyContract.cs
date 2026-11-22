// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Produces AuthProxy's application contract (<c>Cratis:AuthProxy:Application</c>) for a Performance. The structure becomes environment variables; every value that comes from the Cratis environment, and the generated onboarding settings, become files in a mounted Secret that <c>SecretsDirectory</c> points at.
/// </summary>
public static class AuthProxyContract
{
    /// <summary>
    /// Where the Secret of AuthProxy is mounted.
    /// </summary>
    public const string SecretsDirectory = "/run/secrets/cratis";

    /// <summary>
    /// Where the custom pages are mounted.
    /// </summary>
    public const string PagesDirectory = "/mnt/pages";

    /// <summary>
    /// Where the data protection key ring is mounted.
    /// </summary>
    public const string KeysDirectory = "/keys";

    const string Prefix = "Cratis__AuthProxy__Application__";
    const string Root = "Cratis__AuthProxy__";
    const string ServicesPrefix = "Cratis__AuthProxy__Services__application__";

    /// <summary>
    /// Gets the environment variables, which hold structure only and never a value from the Cratis environment.
    /// </summary>
    /// <param name="performance">The Performance.</param>
    /// <returns>The variables by name.</returns>
    public static IReadOnlyDictionary<string, string> EnvironmentVariables(Performance performance)
    {
        var spec = performance.Spec;
        var variables = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [$"{Prefix}Onboarding"] = spec.Onboarding.Enabled ? "true" : "false",

            // Development allows HTTP; nothing in a cluster is allowed to.
            [$"{Prefix}Development"] = "false",
            [$"{Prefix}SecretsDirectory"] = SecretsDirectory
        };

        for (var index = 0; index < spec.AuthProxy.Providers.Count; index++)
        {
            var provider = spec.AuthProxy.Providers[index];
            variables[$"{Prefix}Providers__{index}__Kind"] = provider.Kind.ToString();
            variables[$"{Prefix}Providers__{index}__Identity"] = provider.Identity;
        }

        if (!string.IsNullOrWhiteSpace(spec.Environment.Name))
        {
            variables[$"{Prefix}EnvironmentName"] = spec.Environment.Name;
        }

        for (var index = 0; index < spec.AuthProxy.TrustedProxies.Count; index++)
        {
            variables[$"{Root}Ingress__TrustedProxies__{index}"] = spec.AuthProxy.TrustedProxies[index];
        }

        AddTenancy(variables, spec.AuthProxy.Tenancy);
        AddRequiredClaims(variables, spec.AuthProxy.RequiredClaims);
        if (spec.AuthProxy.Pages is not null)
        {
            variables[$"{Root}PagesPath"] = PagesDirectory;
        }

        if (spec.AuthProxy.DataProtection is not null)
        {
            variables[$"{Root}DataProtectionKeysPath"] = KeysDirectory;
        }

        if (spec.Onboarding.Enabled)
        {
            // The contract only takes HTTPS for its lobby addresses, and Ante is in the cluster: its own Service is
            // where the invitation paths on the public host are routed to. The environment's key still wins.
            variables[$"{Root}Invite__Lobby__Backend__BaseUrl"] = $"http://{PerformanceNames.For(performance).Ante}:{spec.Onboarding.Port}/";
        }

        AddApplicationService(variables, performance);
        return variables;
    }

    /// <summary>
    /// Gets the files of the Secret: one per environment key, the key as the file name.
    /// </summary>
    /// <param name="performance">The Performance.</param>
    /// <param name="environment">The resolved Cratis environment.</param>
    /// <param name="keys">The generated signing keys.</param>
    /// <returns>The file contents by file name.</returns>
    public static IReadOnlyDictionary<string, string> SecretFiles(Performance performance, ResolvedEnvironment environment, SigningKeyState keys)
    {
        var spec = performance.Spec;
        var files = environment.Values
            .Where(pair => EnvironmentKeys.IsForAuthProxy(pair.Key, spec))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);

        // What the spec names wins over the environment: the operator reads it, so it is the one that is current.
        foreach (var provider in spec.AuthProxy.Providers.Where(provider => provider.Scopes.Count > 0))
        {
            files[$"{EnvironmentKeys.ProvidersPrefix}{provider.Identity}.scopes"] = string.Join(',', provider.Scopes);
        }

        if (!spec.Onboarding.Enabled)
        {
            return files;
        }

        // The operator knows these better than the environment does, so they win.
        var publicUrl = PublicEndpoint.AddressOf(spec);
        var identity = OnboardingIdentity.For(performance);
        files["onboarding.stageUrl"] = $"{publicUrl}/_invite/stage";
        files["onboarding.exchangeUrl"] = $"{publicUrl}/_invite/exchange";
        files["onboarding.tokenMetadataUrl"] = $"{publicUrl}/.well-known/ante/invitation-token";
        files["onboarding.issuer"] = identity.Issuer;
        files["onboarding.audience"] = identity.Audience;
        files["onboarding.lobbyFrontendBaseUrl"] = $"{publicUrl}/";
        files["onboarding.attestation.issuer"] = identity.AttestationIssuer;
        files["onboarding.attestation.audience"] = identity.Issuer;
        files["onboarding.attestation.keyId"] = keys.Attestation.Current.KeyId;
        files["onboarding.attestation.privateKeyPem"] = keys.Attestation.Current.PrivateKeyPem;
        return files;
    }

    /// <summary>
    /// Points AuthProxy at the application: it forwards only to the services it is told about, and the application is the one service, backend and frontend alike, reached on its in-cluster Service.
    /// </summary>
    /// <param name="variables">The variables to add to.</param>
    /// <param name="performance">The Performance.</param>
    static void AddApplicationService(Dictionary<string, string> variables, Performance performance)
    {
        if (performance.Spec.AuthProxy.Upstream is { } upstream)
        {
            AddUpstream(variables, upstream);
            return;
        }

        var port = PublishedStageContract.IsPublished(performance.Spec) ? PublishedStageContract.Port : performance.Spec.Application.Port;
        var address = $"http://{PerformanceNames.For(performance).Application}:{port}/";
        variables[$"{ServicesPrefix}Backend__BaseUrl"] = address;
        variables[$"{ServicesPrefix}Frontend__BaseUrl"] = address;
    }

    static void AddUpstream(Dictionary<string, string> variables, AuthProxyUpstreamSpec upstream)
    {
        variables[$"{ServicesPrefix}Frontend__BaseUrl"] = upstream.Url;
        if (upstream.ThirdParty)
        {
            // It has no /.cratis/me to ask who is calling: AuthProxy only decides who may get in.
            variables[$"{ServicesPrefix}ResolveIdentityDetails"] = "false";
            return;
        }

        variables[$"{ServicesPrefix}Backend__BaseUrl"] = upstream.Url;
    }

    static void AddTenancy(Dictionary<string, string> variables, AuthProxyTenancySpec? tenancy)
    {
        const string resolution = $"{Root}TenantResolutions__0__";
        if (!string.IsNullOrWhiteSpace(tenancy?.TenantsEndpoint))
        {
            variables[$"{resolution}Strategy"] = "Selection";
            variables[$"{resolution}Options__TenantsEndpoint"] = tenancy.TenantsEndpoint;
        }
        else if (!string.IsNullOrWhiteSpace(tenancy?.TenantId))
        {
            variables[$"{resolution}Strategy"] = "Default";
            variables[$"{resolution}Options__TenantId"] = tenancy.TenantId;
        }
    }

    static void AddRequiredClaims(Dictionary<string, string> variables, IList<AuthProxyRequiredClaim> claims)
    {
        for (var index = 0; index < claims.Count; index++)
        {
            var prefix = $"{Root}Authorization__RequiredClaims__{index}__";
            variables[$"{prefix}Claim"] = claims[index].Claim;
            for (var value = 0; value < claims[index].AnyOf.Count; value++)
            {
                variables[$"{prefix}AnyOf__{value}"] = claims[index].AnyOf[value];
            }
        }
    }
}
