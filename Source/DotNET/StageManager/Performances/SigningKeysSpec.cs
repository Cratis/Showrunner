// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// How the operator-generated signing keys are rotated.
/// </summary>
public class SigningKeysSpec
{
    /// <summary>
    /// Gets or sets the rotation counter. Raising it past the observed value rotates both keys.
    /// </summary>
    [Description("The rotation counter. Raising it rotates the Ante invitation key and the AuthProxy attestation key; the previous public keys stay pinned.")]
    public int Rotation { get; set; }

    /// <summary>
    /// Gets or sets how many previous public keys stay pinned.
    /// </summary>
    [Description("How many previous public keys stay pinned while their tokens can still be valid.")]
    public int RetainPrevious { get; set; } = 2;
}
