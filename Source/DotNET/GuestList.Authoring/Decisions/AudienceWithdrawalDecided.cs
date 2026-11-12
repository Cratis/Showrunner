// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Authoring.Decisions;

/// <summary>
/// Recorded when an operator decided to withdraw an audience that was held. It is the operator side's own fact, in the
/// stream of the product; the outbox publishes it as <c>AudienceWithdrawn</c>.
/// </summary>
/// <param name="Level">Whom the audience is withdrawn from.</param>
/// <param name="TenantId">The tenant, for <see cref="AudienceLevel.Tenant"/> and <see cref="AudienceLevel.User"/>.</param>
/// <param name="UserId">The user, for <see cref="AudienceLevel.User"/>.</param>
/// <param name="Audience">The audience withdrawn.</param>
[EventType]
public record AudienceWithdrawalDecided(AudienceLevel Level, TenantId TenantId, UserId UserId, AudienceName Audience);
