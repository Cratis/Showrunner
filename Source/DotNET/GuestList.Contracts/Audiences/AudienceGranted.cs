// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Contracts.Audiences;

/// <summary>
/// Published when an operator grants an audience to a tenant, a user or the system. The decision is dated by when it
/// occurred: the newest decision about the same audience at the same level wins, and a grant that arrives twice is
/// recorded once. <see cref="TenantId"/> and <see cref="UserId"/> are empty when the level does not name them.
/// </summary>
/// <param name="Product">The product the decision is for.</param>
/// <param name="Level">Whom the audience is granted to.</param>
/// <param name="TenantId">The tenant, for <see cref="AudienceLevel.Tenant"/> and <see cref="AudienceLevel.User"/>.</param>
/// <param name="UserId">The user, for <see cref="AudienceLevel.User"/>.</param>
/// <param name="Audience">The audience granted.</param>
[EventType]
[Public]
public record AudienceGranted(Product Product, AudienceLevel Level, TenantId TenantId, UserId UserId, AudienceName Audience);
