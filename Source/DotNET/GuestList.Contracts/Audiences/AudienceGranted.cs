// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Contracts.Audiences;

/// <summary>
/// Published when an operator grants an audience to a tenant, a user, the system or one named system. The decision is dated
/// by when it occurred: the newest decision about the same audience at the same level wins, and a grant that arrives twice
/// is recorded once. <see cref="TenantId"/>, <see cref="UserId"/> and <see cref="ClientId"/> are empty when the level does
/// not name them. This is generation 2; generation 1 had no <see cref="ClientId"/> or <see cref="Kind"/>, and an old
/// <see cref="AudienceLevel.System"/> grant still means the product-wide system holder.
/// </summary>
/// <param name="Product">The product the decision is for.</param>
/// <param name="Level">Whom the audience is granted to.</param>
/// <param name="TenantId">The tenant, for <see cref="AudienceLevel.Tenant"/> and <see cref="AudienceLevel.User"/>.</param>
/// <param name="UserId">The user, for <see cref="AudienceLevel.User"/>.</param>
/// <param name="Audience">The audience granted.</param>
/// <param name="ClientId">The client id of the system, for <see cref="AudienceLevel.NamedSystem"/>.</param>
/// <param name="Kind">The kind of the system, for <see cref="AudienceLevel.NamedSystem"/>; <see cref="SystemKind.None"/> otherwise.</param>
[EventType(generation: 2)]
[Public]
public record AudienceGranted(Product Product, AudienceLevel Level, TenantId TenantId, UserId UserId, AudienceName Audience, SystemClientId ClientId, SystemKind Kind);
