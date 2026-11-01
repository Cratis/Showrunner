// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// The keys a Performance needs from its Cratis environment, and the ones each container may see.
/// </summary>
public static class EnvironmentKeys
{
    /// <summary>
    /// The prefix of the keys that configure AuthProxy's identity providers.
    /// </summary>
    public const string ProvidersPrefix = "authentication.providers.";

    /// <summary>
    /// The prefix of the keys AuthProxy reads for onboarding.
    /// </summary>
    public const string OnboardingPrefix = "onboarding.";

    /// <summary>
    /// The key holding the MongoDB server Ante keeps its sessions and read models in.
    /// </summary>
    public const string AnteMongoDbServer = "ante.mongodb.server";

    /// <summary>
    /// The optional onboarding keys the environment may set for AuthProxy. The required ones are derived by the operator.
    /// </summary>
    public static readonly IReadOnlyList<string> OptionalOnboardingKeys =
    [
        "onboarding.lobbyBackendBaseUrl",
        "onboarding.registrationBaseUrl",
        "onboarding.subjectAlreadyExistsUrl",
        "onboarding.tenantClaim",
        "onboarding.emailClaim",
        "onboarding.attestation.lifetimeSeconds"
    ];

    /// <summary>
    /// Gets the keys the environment must have for a Performance to roll out.
    /// </summary>
    /// <param name="spec">The spec of the Performance.</param>
    /// <returns>The required keys.</returns>
    public static IReadOnlyList<string> RequiredBy(PerformanceSpec spec)
    {
        var keys = new List<string>();
        if (spec.AuthProxy.Enabled)
        {
            foreach (var provider in spec.AuthProxy.Providers)
            {
                keys.AddRange(RequiredFor(provider).Select(key => $"{ProvidersPrefix}{provider.Identity}.{key}"));
            }
        }

        if (spec.Onboarding.Enabled)
        {
            keys.Add(AnteMongoDbServer);
        }

        return keys;
    }

    /// <summary>
    /// Gets the keys, without prefix, one identity provider needs.
    /// </summary>
    /// <param name="provider">The provider.</param>
    /// <returns>The required keys.</returns>
    public static IReadOnlyList<string> RequiredFor(AuthProxyProvider provider) => provider.Kind switch
    {
        AuthProxyProviderKind.EntraId => ["tenantId", "clientId", "clientSecret"],
        AuthProxyProviderKind.OpenId => ["authority", "clientId", "clientSecret"],
        _ => ["clientId", "clientSecret"]
    };

    /// <summary>
    /// Determines whether a key is for one of the declared providers.
    /// </summary>
    /// <param name="key">The environment key.</param>
    /// <param name="spec">The spec.</param>
    /// <returns>True when AuthProxy should receive the key.</returns>
    public static bool IsForAuthProxy(string key, PerformanceSpec spec) =>
        spec.AuthProxy.Providers.Any(provider => key.StartsWith($"{ProvidersPrefix}{provider.Identity}.", StringComparison.Ordinal)) ||
        OptionalOnboardingKeys.Contains(key, StringComparer.Ordinal);

    /// <summary>
    /// Determines whether a key is for the application itself: everything that is neither AuthProxy's nor Ante's.
    /// </summary>
    /// <param name="key">The environment key.</param>
    /// <returns>True when the application should receive the key.</returns>
    public static bool IsForApplication(string key) =>
        !key.StartsWith(ProvidersPrefix, StringComparison.Ordinal) &&
        !key.StartsWith(OnboardingPrefix, StringComparison.Ordinal) &&
        !key.StartsWith("ante.", StringComparison.Ordinal);
}
