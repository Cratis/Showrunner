// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Finds what is wrong with the AuthProxy settings of a <see cref="Performance"/>, the gateway mode included.
/// </summary>
public static class AuthProxyValidator
{
    /// <summary>
    /// Adds the problems with the AuthProxy settings.
    /// </summary>
    /// <param name="spec">The spec.</param>
    /// <param name="problems">The list to add to.</param>
    public static void Validate(PerformanceSpec spec, ICollection<string> problems)
    {
        var proxy = spec.AuthProxy;
        ValidateGateway(spec, problems);
        ValidateTenancy(proxy.Tenancy, problems);
        ValidateRequiredClaims(proxy.RequiredClaims, problems);
        ValidateProviderDetails(proxy.Providers, problems);

        if (proxy.Pages is { } pages && string.IsNullOrWhiteSpace(pages.ConfigMapRef.Name))
        {
            problems.Add("authProxy.pages.configMapRef.name is required.");
        }

        if (proxy.DataProtection is { } protection)
        {
            if (string.IsNullOrWhiteSpace(protection.ClaimName))
            {
                problems.Add("authProxy.dataProtection.claimName is required.");
            }

            if (proxy.Replicas != 1)
            {
                problems.Add("authProxy.replicas must be 1 with authProxy.dataProtection: the key ring is one ReadWriteOnce claim.");
            }
        }
    }

    static void ValidateGateway(PerformanceSpec spec, ICollection<string> problems)
    {
        if (spec.AuthProxy.Upstream is not { } upstream)
        {
            return;
        }

        if (!spec.AuthProxy.Enabled)
        {
            problems.Add("authProxy.upstream needs authProxy.enabled: a gateway is AuthProxy in front of a Service.");
        }

        if (!IsHttpUrl(upstream.Url))
        {
            problems.Add("authProxy.upstream.url must be an absolute http or https URL.");
        }

        if (spec.Onboarding.Enabled)
        {
            problems.Add("authProxy.upstream and onboarding exclude each other: a gateway has no Ante.");
        }

        if (spec.ProductionRef is not null || spec.Chronicle is not null)
        {
            problems.Add("authProxy.upstream and productionRef or chronicle exclude each other: a gateway does not use Chronicle.");
        }

        if (!string.IsNullOrWhiteSpace(spec.Application.Image) || spec.Application.Published is not null || spec.Application.Sinks.Count > 0 || spec.Application.Graph || spec.Application.EventStores.Count > 0)
        {
            problems.Add("authProxy.upstream and application exclude each other: a gateway runs no application.");
        }
    }

    static void ValidateTenancy(AuthProxyTenancySpec? tenancy, ICollection<string> problems)
    {
        if (tenancy is null)
        {
            return;
        }

        var hasTenant = !string.IsNullOrWhiteSpace(tenancy.TenantId);
        var hasEndpoint = !string.IsNullOrWhiteSpace(tenancy.TenantsEndpoint);
        if (hasTenant && hasEndpoint)
        {
            problems.Add("authProxy.tenancy takes tenantId or tenantsEndpoint, not both.");
        }
        else if (!hasTenant && !hasEndpoint)
        {
            problems.Add("authProxy.tenancy needs tenantId or tenantsEndpoint; omit it to leave tenancy to AuthProxy.");
        }
        else if (hasEndpoint && !IsHttpUrl(tenancy.TenantsEndpoint!))
        {
            problems.Add("authProxy.tenancy.tenantsEndpoint must be an absolute http or https URL.");
        }
    }

    static void ValidateRequiredClaims(IList<AuthProxyRequiredClaim> claims, ICollection<string> problems)
    {
        foreach (var claim in claims)
        {
            if (string.IsNullOrWhiteSpace(claim.Claim))
            {
                problems.Add("authProxy.requiredClaims may not hold an entry without a claim.");
            }
            else if (claim.AnyOf.Any(string.IsNullOrWhiteSpace))
            {
                problems.Add($"authProxy.requiredClaims '{claim.Claim}' may not hold an empty value.");
            }
        }
    }

    static void ValidateProviderDetails(IList<AuthProxyProvider> providers, ICollection<string> problems)
    {
        foreach (var provider in providers)
        {
            if (provider.Scopes.Any(scope => string.IsNullOrWhiteSpace(scope) || scope.Any(char.IsWhiteSpace) || scope.Contains(',', StringComparison.Ordinal)))
            {
                problems.Add($"The scopes of the provider '{provider.Identity}' must each be one word without commas.");
            }

            if (provider.CredentialsSecretRef is { } credentials && (string.IsNullOrWhiteSpace(credentials.Name) || string.IsNullOrWhiteSpace(credentials.ClientIdKey) || string.IsNullOrWhiteSpace(credentials.ClientSecretKey)))
            {
                problems.Add($"The credentialsSecretRef of the provider '{provider.Identity}' needs a name, clientIdKey and clientSecretKey.");
            }
        }
    }

    static bool IsHttpUrl(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var url) && (url.Scheme == Uri.UriSchemeHttp || url.Scheme == Uri.UriSchemeHttps);
}
