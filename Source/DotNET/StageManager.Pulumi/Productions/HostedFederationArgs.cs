// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;

namespace Cratis.Showrunner.StageManager.Pulumi.Productions;

/// <summary>
/// The Chronicle of a Production acting as the certificate authority for the federation links of its customers.
/// It needs <see cref="ProductionFederationArgs.Enabled"/>.
/// </summary>
public sealed class HostedFederationArgs
{
    /// <summary>
    /// Gets a value indicating whether Chronicle acts as the authority and accepts enrollments. Defaults to true.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets the https address customers reach the federation listener on, such as <c>https://federation.example.com:35100</c>.
    /// </summary>
    public Input<string>? PublicEndpoint { get; init; }

    /// <summary>
    /// Gets the trust domain of the authority, such as <c>example</c>.
    /// </summary>
    public string? TrustDomain { get; init; }

    /// <summary>
    /// Builds the spec of the hosted federation.
    /// </summary>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec() =>
        SpecExtensions.NewSpec()
            .Put("enabled", Enabled)
            .Put("publicEndpoint", PublicEndpoint)
            .Put("trustDomain", TrustDomain);
}
