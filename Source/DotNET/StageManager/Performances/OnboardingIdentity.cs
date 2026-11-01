// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// The identities an application's Ante and AuthProxy agree on, derived from the Performance so nothing has to be written twice.
/// </summary>
/// <param name="EventStore">Ante's own event store.</param>
/// <param name="Namespace">The Chronicle namespace Ante runs in.</param>
/// <param name="Issuer">The issuer of invitation tokens.</param>
/// <param name="Audience">The audience of invitation tokens.</param>
/// <param name="AttestationIssuer">The issuer of AuthProxy's attestations.</param>
/// <param name="LobbyScope">The authentication scope of the lobby.</param>
public record OnboardingIdentity(string EventStore, string Namespace, string Issuer, string Audience, string AttestationIssuer, string LobbyScope)
{
    /// <summary>
    /// Derives the identities of a Performance. The token issuer follows Ante's own rule, <c>urn:cratis:ante:{event store}:{namespace}</c>, with each name percent-encoded.
    /// </summary>
    /// <param name="performance">The Performance.</param>
    /// <returns>The identities.</returns>
    public static OnboardingIdentity For(Performance performance)
    {
        var name = performance.Metadata.Name;
        var onboarding = performance.Spec.Onboarding;
        var eventStore = string.IsNullOrWhiteSpace(onboarding.EventStore) ? $"ante-{name}" : onboarding.EventStore;
        var issuer = $"urn:cratis:ante:{Uri.EscapeDataString(eventStore)}:{Uri.EscapeDataString(onboarding.Namespace)}";
        return new(eventStore, onboarding.Namespace, issuer, $"{issuer}:lobby", $"urn:cratis:authproxy:{name}", name);
    }
}
