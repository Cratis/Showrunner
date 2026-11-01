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

    const string Prefix = "Cratis__AuthProxy__Application__";

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
            [$"{Prefix}EnvironmentName"] = spec.Environment.Name,
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
}
