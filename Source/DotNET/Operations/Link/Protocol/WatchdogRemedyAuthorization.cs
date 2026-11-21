// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// A remedy request Direct authorized. The instance must still re-check target state, revision, epoch, idempotency and
/// cooldown before executing, and must not execute after <see cref="ExpiresAt"/>.
/// </summary>
/// <param name="Request">The request.</param>
/// <param name="Remedy">The remedy's name.</param>
/// <param name="Resource">The resource.</param>
/// <param name="ConfigurationRevision">The revision it was authorized under.</param>
/// <param name="FencingEpoch">The epoch it was authorized for.</param>
/// <param name="ExpiresAt">When the authorization lapses.</param>
public record WatchdogRemedyAuthorization(Guid Request, string Remedy, Guid Resource, long ConfigurationRevision, long FencingEpoch, DateTimeOffset ExpiresAt);
