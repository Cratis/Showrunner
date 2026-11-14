// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Authoring.Decisions;

/// <summary>
/// Recorded when an operator decided to grant an audience that was not held. It is the operator side's own fact, in the
/// stream of the product; the outbox publishes it as <c>AudienceGranted</c>.
/// </summary>
/// <param name="Level">Whom the audience is granted to.</param>
/// <param name="TenantId">The tenant, for <see cref="AudienceLevel.Tenant"/> and <see cref="AudienceLevel.User"/>.</param>
/// <param name="UserId">The user, for <see cref="AudienceLevel.User"/>.</param>
/// <param name="Audience">The audience granted.</param>
/// <param name="ClientId">The client id of the system, for <see cref="AudienceLevel.NamedSystem"/>.</param>
/// <param name="Kind">The kind of the system, for <see cref="AudienceLevel.NamedSystem"/>.</param>
[EventType(generation: 2)]
public record AudienceGrantDecided(AudienceLevel Level, TenantId TenantId, UserId UserId, AudienceName Audience, SystemClientId ClientId, SystemKind Kind);
