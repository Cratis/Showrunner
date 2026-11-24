// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Authoring.Decisions;

/// <summary>
/// A part an operator has suspended and not resumed.
/// </summary>
/// <param name="Level">Whom the part is suspended for.</param>
/// <param name="TenantId">The tenant, for <see cref="SuspensionLevel.Tenant"/> and <see cref="SuspensionLevel.User"/>; empty otherwise.</param>
/// <param name="UserId">The user, for <see cref="SuspensionLevel.User"/>; empty otherwise.</param>
/// <param name="Part">The path of the part.</param>
/// <param name="Since">When the operator suspended it.</param>
public record HeldSuspension(SuspensionLevel Level, TenantId TenantId, UserId UserId, PartPath Part, DateTimeOffset Since)
{
    /// <summary>
    /// Gets whether another suspension is held at the same place: the same level, tenant, user and part.
    /// </summary>
    /// <param name="level">The level.</param>
    /// <param name="tenant">The tenant.</param>
    /// <param name="user">The user.</param>
    /// <param name="part">The part.</param>
    /// <returns><see langword="true"/> when it is the same place; otherwise <see langword="false"/>.</returns>
    public bool IsAt(SuspensionLevel level, TenantId tenant, UserId user, PartPath part) =>
        Level == level &&
        Part == part &&
        (TenantId?.Value ?? string.Empty) == (tenant?.Value ?? string.Empty) &&
        (UserId?.Value ?? string.Empty) == (user?.Value ?? string.Empty);
}
