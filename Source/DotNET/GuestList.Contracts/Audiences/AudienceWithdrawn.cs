// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Contracts.Audiences;

/// <summary>
/// Published when an operator withdraws an audience from a tenant, a user or the system. It removes the grant of its own
/// level only. <see cref="TenantId"/> and <see cref="UserId"/> are empty when the level does not name them.
/// </summary>
/// <param name="Product">The product the decision is for.</param>
/// <param name="Level">Whom the audience is withdrawn from.</param>
/// <param name="TenantId">The tenant, for <see cref="AudienceLevel.Tenant"/> and <see cref="AudienceLevel.User"/>.</param>
/// <param name="UserId">The user, for <see cref="AudienceLevel.User"/>.</param>
/// <param name="Audience">The audience withdrawn.</param>
[EventType]
[Public]
public record AudienceWithdrawn(Product Product, AudienceLevel Level, TenantId TenantId, UserId UserId, AudienceName Audience);
