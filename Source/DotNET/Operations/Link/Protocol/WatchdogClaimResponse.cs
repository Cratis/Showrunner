// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// What a successful claim returns.
/// </summary>
/// <param name="InstanceName">The instance's operator-facing name.</param>
/// <param name="EventStore">The event store the instance keeps its own events in.</param>
/// <param name="FencingEpoch">The epoch every later request must present in <see cref="WatchdogProtocol.EpochHeader"/>.</param>
/// <param name="Credential">The instance credential - returned only by the first claim; empty on every later claim.</param>
/// <param name="SigningKey">The base64 HMAC-SHA256 key configuration snapshots are signed with.</param>
/// <param name="LeaseExpiresAt">When the claim lapses without a heartbeat.</param>
public record WatchdogClaimResponse(
    string InstanceName,
    string EventStore,
    long FencingEpoch,
    string Credential,
    string SigningKey,
    DateTimeOffset LeaseExpiresAt);
