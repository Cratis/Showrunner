// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Productions;

/// <summary>
/// How the Chronicle of a Production takes part in federation: serving a listener, acting as the authority customers enroll
/// with, or dialing out to an authority.
/// </summary>
public sealed class ProductionFederationArgs
{
    /// <summary>
    /// Gets a value indicating whether Chronicle opens the federation listener and runs consumer bindings. Off by default.
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// Gets the port of the federation listener, or null for the operator's default (35100).
    /// </summary>
    public int? Port { get; init; }

    /// <summary>
    /// Gets the settings that make Chronicle the certificate authority for its customers' links. Needs <see cref="Enabled"/>.
    /// </summary>
    public HostedFederationArgs? Hosted { get; init; }

    /// <summary>
    /// Gets the settings that make Chronicle dial an authority it enrolled with.
    /// </summary>
    public FederationDialArgs? Dial { get; init; }

    /// <summary>
    /// Builds the spec of the federation.
    /// </summary>
    /// <param name="enrollmentCodeSecret">The Secret holding the enrollment code, if any.</param>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec(SecretReferenceArgs? enrollmentCodeSecret) =>
        SpecExtensions.NewSpec()
            .Put("enabled", Enabled)
            .Put("port", Port)
            .Put("hosted", Hosted?.ToSpec())
            .Put("dial", Dial?.ToSpec(enrollmentCodeSecret));
}
