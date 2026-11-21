// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Operations.Link.Protocol;

namespace Cratis.Showrunner.Operations.Link;

/// <summary>
/// What the link keeps in its Kubernetes Secret: everything that is a credential or derived from one. It never reaches a log,
/// a finding or the status.
/// </summary>
/// <param name="Session">The value this watchdog presents as its session in every claim; kept across passes, so Direct sees one process.</param>
/// <param name="Credential">The instance credential that replaced the enrollment token.</param>
/// <param name="SigningKey">The base64 key configuration is verified with, pinned at the first claim.</param>
/// <param name="Epoch">The fencing epoch of the last confirmed claim.</param>
/// <param name="Held">The signed configuration currently held - it contains the secrets its checks use.</param>
/// <param name="AcknowledgeRevision">A revision applied but not yet acknowledged to Direct, or zero.</param>
/// <param name="AcknowledgeDigest">The digest of that revision.</param>
/// <param name="ConfirmedAt">When Direct last confirmed the claim - how long ago decides whether the epoch can still be trusted offline.</param>
public sealed record LinkIdentity(
    string Session,
    string Credential,
    string SigningKey,
    long Epoch,
    SignedWatchdogConfiguration? Held,
    long AcknowledgeRevision = 0,
    string AcknowledgeDigest = "",
    DateTimeOffset? ConfirmedAt = null);
