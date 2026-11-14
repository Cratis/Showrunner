// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Contracts.Audiences;

/// <summary>
/// Generation 1 of <see cref="AudienceWithdrawn"/>, as it shipped: no system is named, so a system withdrawal is the
/// product-wide one.
/// </summary>
/// <param name="Product">The product the decision is for.</param>
/// <param name="Level">Whom the audience is withdrawn from.</param>
/// <param name="TenantId">The tenant, for the tenant and user levels.</param>
/// <param name="UserId">The user, for the user level.</param>
/// <param name="Audience">The audience withdrawn.</param>
[EventTypeGenerationFor<AudienceWithdrawn>(1)]
public record AudienceWithdrawnV1(Product Product, AudienceLevelV1 Level, TenantId TenantId, UserId UserId, AudienceName Audience);
