// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.GuestList.Contracts.Audiences;

namespace Cratis.Showrunner.GuestList.Contracts.Suspensions;

/// <summary>
/// Published when an operator resumes a part that was suspended for a tenant, a user or the system. It ends the suspension
/// of its own level only. The decision is dated by when it occurred: the newest decision about the same part at the same
/// level wins. <see cref="TenantId"/> and <see cref="UserId"/> are empty when the level does not name them.
/// </summary>
/// <param name="Product">The product the decision is for.</param>
/// <param name="Level">Whom the part is resumed for.</param>
/// <param name="TenantId">The tenant, for <see cref="SuspensionLevel.Tenant"/> and <see cref="SuspensionLevel.User"/>.</param>
/// <param name="UserId">The user, for <see cref="SuspensionLevel.User"/>.</param>
/// <param name="Part">The path of the part resumed.</param>
[EventType]
[Public]
public record PartResumed(Product Product, SuspensionLevel Level, TenantId TenantId, UserId UserId, PartPath Part);
