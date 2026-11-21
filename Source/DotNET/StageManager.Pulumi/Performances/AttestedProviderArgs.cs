// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Performances;

/// <summary>
/// An identity provider whose attestations Ante's invitation exchange accepts.
/// </summary>
public sealed class AttestedProviderArgs
{
    /// <summary>
    /// Gets the provider key: the lowercase identity of the provider.
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// Gets the issuer the provider's tokens carry.
    /// </summary>
    public required string Issuer { get; init; }

    /// <summary>
    /// Gets the assurances the exchange accepts, or none to accept any.
    /// </summary>
    public IList<string> AcceptableAssurances { get; init; } = [];

    /// <summary>
    /// Builds the spec of the attested provider.
    /// </summary>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec() =>
        SpecExtensions.NewSpec()
            .Put("key", Key)
            .Put("issuer", Issuer)
            .PutList("acceptableAssurances", AcceptableAssurances);
}
