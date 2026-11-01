// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// The public side of the generated signing keys. Private keys live only in Secrets.
/// </summary>
public class SigningKeysStatus
{
    /// <summary>
    /// Gets or sets the id of the key Ante signs invitations with.
    /// </summary>
    [Description("The id of the key Ante signs invitations with.")]
    public string InvitationKeyId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the id of the key AuthProxy signs attestations with.
    /// </summary>
    [Description("The id of the key AuthProxy signs attestations with.")]
    public string AttestationKeyId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the rotation counter that was last applied.
    /// </summary>
    [Description("The rotation counter that was last applied.")]
    public int ObservedRotation { get; set; }

    /// <summary>
    /// Gets or sets how many previous public keys are pinned.
    /// </summary>
    [Description("How many previous public keys are pinned.")]
    public int PinnedPreviousKeys { get; set; }
}
