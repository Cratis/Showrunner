// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Authoring.Decisions;

/// <summary>
/// Generation 1 of <see cref="AudienceGrantDecided"/>, as it was stored: no system is named, so a system grant is the
/// product-wide one.
/// </summary>
/// <param name="Level">Whom the audience is granted to.</param>
/// <param name="TenantId">The tenant, for the tenant and user levels.</param>
/// <param name="UserId">The user, for the user level.</param>
/// <param name="Audience">The audience granted.</param>
[EventTypeGenerationFor<AudienceGrantDecided>(1)]
public record AudienceGrantDecidedV1(AudienceLevelV1 Level, TenantId TenantId, UserId UserId, AudienceName Audience);
