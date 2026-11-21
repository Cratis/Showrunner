// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// Why a delivered configuration snapshot was or was not applied.
/// </summary>
public enum ConfigurationAcceptanceReason
{
    /// <summary>
    /// It is signed, meant for this instance, newer and unexpired - apply it atomically.
    /// </summary>
    Accepted = 0,

    /// <summary>
    /// The signature does not verify under the instance's key.
    /// </summary>
    InvalidSignature = 1,

    /// <summary>
    /// It was issued for a different instance.
    /// </summary>
    WrongInstance = 2,

    /// <summary>
    /// Its revision is not newer than the one already applied - stale or replayed.
    /// </summary>
    NotNewer = 3,

    /// <summary>
    /// It expired before it arrived.
    /// </summary>
    Expired = 4,

    /// <summary>
    /// Its digest does not match its content.
    /// </summary>
    DigestMismatch = 5,

    /// <summary>
    /// It is the revision already applied with the same content and a later lease - a lease renewal.
    /// </summary>
    Renewed = 6,
}
