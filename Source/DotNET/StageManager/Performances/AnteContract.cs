// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Produces Ante's onboarding contract (<c>Ante:Onboarding</c>) for a Performance. Structure becomes environment variables. Everything secret, and everything that comes from the Cratis environment, becomes a file in a mounted folder, named like the setting it configures.
/// </summary>
public static class AnteContract
{
    /// <summary>
    /// Where the Secret of Ante is mounted.
    /// </summary>
    public const string SecretsDirectory = "/run/secrets/ante";

    /// <summary>
    /// Where the legal terms are mounted.
    /// </summary>
    public const string LegalDirectory = "/etc/ante/legal";

    /// <summary>
    /// Gets the environment variables, which hold structure only.
    /// </summary>
    /// <param name="performance">The Performance.</param>
    /// <returns>The variables by name.</returns>
    public static IReadOnlyDictionary<string, string> EnvironmentVariables(Performance performance)
    {
        var spec = performance.Spec;
        var onboarding = spec.Onboarding;
        var identity = OnboardingIdentity.For(performance);
        var publicUrl = PublicEndpoint.AddressOf(spec);
        var registration = onboarding.SignUp == SignUpMode.CreateTenant && onboarding.RegistrationEnabled;

        // Development mode is never set: Ante refuses it when it finds itself in a pod.
        var variables = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Ante__Onboarding__Enabled"] = "true",
            ["Ante__Onboarding__HostStore"] = spec.Application.EventStores[0],
            ["Ante__Onboarding__EventStore"] = identity.EventStore,
            ["Ante__Onboarding__Namespace"] = identity.Namespace,
            ["Ante__Onboarding__PublicBaseUrl"] = publicUrl,
            ["Ante__Onboarding__ApplicationUrl"] = $"{publicUrl}/",
            ["Ante__Onboarding__SignUp"] = onboarding.SignUp.ToString(),
            ["Ante__Onboarding__RegistrationEnabled"] = registration ? "true" : "false",
            ["Ante__Onboarding__DefaultLocale"] = onboarding.DefaultLocale,
            ["Ante__Onboarding__Secrets__Path"] = SecretsDirectory,
            ["Ante__Invitations__Token__Issuer"] = identity.Issuer,
            ["Ante__Invitations__Token__Audience"] = identity.Audience,
            ["Cratis__MongoDB__Database"] = identity.EventStore,
            ["ASPNETCORE_ENVIRONMENT"] = "Production"
        };

        AddList(variables, "Ante__Onboarding__InvitationTypes", onboarding.InvitationTypes);
        AddList(variables, "Ante__Onboarding__Personas", onboarding.Personas);
        AddList(variables, "Ante__Onboarding__AllowedLocales", onboarding.AllowedLocales);
        if (onboarding.Legal is not null)
        {
            variables["Ante__Onboarding__Legal__Folder"] = LegalDirectory;
        }

        if (onboarding.AttestedProviders.Count > 0)
        {
            AddAttestation(variables, performance, identity);
        }

        return variables;
    }

    /// <summary>
    /// Gets the files of the Secret: one per setting, named like the setting with <c>__</c> for <c>:</c>.
    /// </summary>
    /// <param name="performance">The Performance.</param>
    /// <param name="environment">The resolved Cratis environment.</param>
    /// <param name="keys">The generated signing keys.</param>
    /// <returns>The file contents by file name.</returns>
    public static IReadOnlyDictionary<string, string> SecretFiles(Performance performance, ResolvedEnvironment environment, SigningKeyState keys)
    {
        var files = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Ante__Invitations__Token__PrivateKeyPem"] = keys.Invitation.Current.PrivateKeyPem,
            ["Cratis__MongoDB__Server"] = environment.Values[EnvironmentKeys.AnteMongoDbServer]
        };

        for (var index = 0; index < keys.Invitation.Previous.Count; index++)
        {
            files[$"Ante__Invitations__Token__PreviousPublicKeyPems__{index}"] = keys.Invitation.Previous[index].PublicKeyPem;
        }

        if (performance.Spec.Onboarding.AttestedProviders.Count > 0)
        {
            // Pin the key AuthProxy signs with now and every previous one whose attestations may still be in flight.
            PublicKeyMaterial[] pinned = [new(keys.Attestation.Current.KeyId, keys.Attestation.Current.PublicKeyPem), .. keys.Attestation.Previous];
            for (var index = 0; index < pinned.Length; index++)
            {
                files[$"Ante__Invitations__Exchange__Attestation__PublicKeys__{index}__KeyId"] = pinned[index].KeyId;
                files[$"Ante__Invitations__Exchange__Attestation__PublicKeys__{index}__PublicKeyPem"] = pinned[index].PublicKeyPem;
            }
        }

        return files;
    }

    static void AddAttestation(Dictionary<string, string> variables, Performance performance, OnboardingIdentity identity)
    {
        const string exchange = "Ante__Invitations__Exchange__";
        variables[$"{exchange}Mode"] = "Attested";
        variables[$"{exchange}Attestation__Issuer"] = identity.AttestationIssuer;
        variables[$"{exchange}Attestation__Audience"] = identity.Issuer;
        variables[$"{exchange}Attestation__LobbyScope"] = identity.LobbyScope;

        var providers = performance.Spec.Onboarding.AttestedProviders;
        for (var index = 0; index < providers.Count; index++)
        {
            variables[$"{exchange}Attestation__Providers__{index}__Key"] = providers[index].Key;
            variables[$"{exchange}Attestation__Providers__{index}__Issuer"] = providers[index].Issuer;
            AddList(variables, $"{exchange}Attestation__Providers__{index}__AcceptableAssurances", providers[index].AcceptableAssurances);
        }
    }

    static void AddList(Dictionary<string, string> variables, string name, IList<string> values)
    {
        for (var index = 0; index < values.Count; index++)
        {
            variables[$"{name}__{index}"] = values[index];
        }
    }
}
