// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Performances;

/// <summary>
/// How the signing keys the operator generates for Ante and AuthProxy are rotated.
/// </summary>
public sealed class SigningKeysArgs
{
    /// <summary>
    /// Gets the rotation counter. Raising it past the observed value rotates both keys.
    /// </summary>
    public int? Rotation { get; init; }

    /// <summary>
    /// Gets how many previous public keys stay pinned, or null for the operator's default.
    /// </summary>
    public int? RetainPrevious { get; init; }

    /// <summary>
    /// Builds the spec of the signing keys.
    /// </summary>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec() =>
        SpecExtensions.NewSpec()
            .Put("rotation", Rotation)
            .Put("retainPrevious", RetainPrevious);
}
