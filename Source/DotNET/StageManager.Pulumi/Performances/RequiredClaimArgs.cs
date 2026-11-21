// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Performances;

/// <summary>
/// A claim the signed-in caller must carry before AuthProxy forwards anything, for example <c>urn:github:team</c>.
/// </summary>
public sealed class RequiredClaimArgs
{
    /// <summary>
    /// Gets the claim type.
    /// </summary>
    public required string Claim { get; init; }

    /// <summary>
    /// Gets the values of which the claim must carry one, or none to require only that the claim is present.
    /// </summary>
    public IList<string> AnyOf { get; init; } = [];

    /// <summary>
    /// Builds the spec of the claim.
    /// </summary>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec() =>
        SpecExtensions.NewSpec()
            .Put("claim", Claim)
            .PutList("anyOf", AnyOf);
}
