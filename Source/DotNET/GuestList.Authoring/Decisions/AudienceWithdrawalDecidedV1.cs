// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Authoring.Decisions;

/// <summary>
/// Generation 1 of <see cref="AudienceWithdrawalDecided"/>, as it was stored: no system is named, so a system withdrawal is
/// the product-wide one.
/// </summary>
/// <param name="Level">Whom the audience is withdrawn from.</param>
/// <param name="TenantId">The tenant, for the tenant and user levels.</param>
/// <param name="UserId">The user, for the user level.</param>
/// <param name="Audience">The audience withdrawn.</param>
[EventTypeGenerationFor<AudienceWithdrawalDecided>(1)]
public record AudienceWithdrawalDecidedV1(AudienceLevelV1 Level, TenantId TenantId, UserId UserId, AudienceName Audience);
