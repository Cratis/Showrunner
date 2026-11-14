// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Contracts.Audiences;

/// <summary>
/// Published when an operator withdraws an audience from a tenant, a user, the system or one named system. It removes the
/// grant of its own level only. <see cref="TenantId"/>, <see cref="UserId"/> and <see cref="ClientId"/> are empty when the
/// level does not name them. This is generation 2; generation 1 had no <see cref="ClientId"/> or <see cref="Kind"/>.
/// </summary>
/// <param name="Product">The product the decision is for.</param>
/// <param name="Level">Whom the audience is withdrawn from.</param>
/// <param name="TenantId">The tenant, for <see cref="AudienceLevel.Tenant"/> and <see cref="AudienceLevel.User"/>.</param>
/// <param name="UserId">The user, for <see cref="AudienceLevel.User"/>.</param>
/// <param name="Audience">The audience withdrawn.</param>
/// <param name="ClientId">The client id of the system, for <see cref="AudienceLevel.NamedSystem"/>.</param>
/// <param name="Kind">The kind of the system, for <see cref="AudienceLevel.NamedSystem"/>; <see cref="SystemKind.None"/> otherwise.</param>
[EventType(generation: 2)]
[Public]
public record AudienceWithdrawn(Product Product, AudienceLevel Level, TenantId TenantId, UserId UserId, AudienceName Audience, SystemClientId ClientId, SystemKind Kind);
