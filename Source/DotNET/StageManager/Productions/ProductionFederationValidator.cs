// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// Finds what is wrong with the <see cref="ProductionFederation"/> of a Production.
/// </summary>
internal static class ProductionFederationValidator
{
    static readonly int[] _reservedPorts = [ChronicleWorkload.Port, ChronicleWorkload.SiloPort, ChronicleWorkload.GatewayPort];

    /// <summary>
    /// Validates the federation of a Production.
    /// </summary>
    /// <param name="federation">The federation.</param>
    /// <param name="problems">The problems to add to.</param>
    internal static void Validate(ProductionFederation federation, List<ProductionProblem> problems)
    {
        if (federation.Enabled && (federation.Port is < 1 or > 65535 || _reservedPorts.Contains(federation.Port)))
        {
            problems.Add(new($"federation.port must be a port from 1 to 65535 that Chronicle does not use for anything else, not {federation.Port}."));
        }

        if (federation.Hosted.Enabled)
        {
            ValidateHosted(federation, problems);
        }

        if (federation.Dial.Enabled)
        {
            ValidateDial(federation.Dial, problems);
        }
    }

    static void ValidateHosted(ProductionFederation federation, List<ProductionProblem> problems)
    {
        if (!federation.Enabled)
        {
            problems.Add(new("federation.hosted.enabled needs federation.enabled: hosted federation is served on the federation listener."));
        }

        if (!Uri.TryCreate(federation.Hosted.PublicEndpoint, UriKind.Absolute, out var endpoint) || endpoint.Scheme != Uri.UriSchemeHttps)
        {
            problems.Add(new("federation.hosted.publicEndpoint must be the https address customers reach the listener on, such as https://federation.example.com:35100."));
        }

        if (string.IsNullOrWhiteSpace(federation.Hosted.TrustDomain))
        {
            problems.Add(new("federation.hosted.trustDomain is required."));
        }
    }

    static void ValidateDial(ProductionFederationDial dial, List<ProductionProblem> problems)
    {
        if (string.IsNullOrWhiteSpace(dial.EventStore))
        {
            problems.Add(new("federation.dial.eventStore is required: the local event store whose inbox receives events and whose outbox is served."));
        }

        if (string.IsNullOrWhiteSpace(dial.EnrollmentCodeSecretRef?.Name))
        {
            problems.Add(new("federation.dial.enrollmentCodeSecretRef must name the Secret holding the enrollment code the authority issued."));
        }
    }
}
