// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.GuestList.Contracts.Audiences;

namespace Cratis.Showrunner.GuestList.Contracts.Suspensions;

/// <summary>
/// Published when an operator suspends a part for a tenant, a user or the system. The decision is dated by when it
/// occurred: the newest decision about the same part at the same level wins, and a suspension that arrives twice is
/// recorded once. A suspended part is refused, and a suspended module or feature suspends everything under it.
/// <see cref="TenantId"/> and <see cref="UserId"/> are empty when the level does not name them.
/// </summary>
/// <param name="Product">The product the decision is for.</param>
/// <param name="Level">Whom the part is suspended for.</param>
/// <param name="TenantId">The tenant, for <see cref="SuspensionLevel.Tenant"/> and <see cref="SuspensionLevel.User"/>.</param>
/// <param name="UserId">The user, for <see cref="SuspensionLevel.User"/>.</param>
/// <param name="Part">The path of the part suspended.</param>
[EventType]
[Public]
public record PartSuspended(Product Product, SuspensionLevel Level, TenantId TenantId, UserId UserId, PartPath Part);
