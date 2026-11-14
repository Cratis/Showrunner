// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Authoring.Decisions;

/// <summary>
/// An audience an operator has granted and not withdrawn.
/// </summary>
/// <param name="Level">Whom the audience is granted to.</param>
/// <param name="TenantId">The tenant, for <see cref="AudienceLevel.Tenant"/> and <see cref="AudienceLevel.User"/>; empty otherwise.</param>
/// <param name="UserId">The user, for <see cref="AudienceLevel.User"/>; empty otherwise.</param>
/// <param name="Audience">The audience.</param>
/// <param name="ClientId">The client id of the system, for <see cref="AudienceLevel.NamedSystem"/>; empty otherwise.</param>
/// <param name="Kind">The kind of the system, for <see cref="AudienceLevel.NamedSystem"/>; <see cref="SystemKind.None"/> otherwise.</param>
public record HeldAudience(AudienceLevel Level, TenantId TenantId, UserId UserId, AudienceName Audience, SystemClientId ClientId, SystemKind Kind)
{
    /// <summary>
    /// Gets whether another audience is held at the same place: the same level, tenant, user and name, and for a named
    /// system the same client id. The kind describes a system and is not part of who it is. A grant stored before systems
    /// were named has no client id, which is the same as an empty one.
    /// </summary>
    /// <param name="other">The other audience.</param>
    /// <returns><see langword="true"/> when both are held at the same place; otherwise <see langword="false"/>.</returns>
    public bool IsSameAs(HeldAudience other) =>
        Level == other.Level &&
        Audience == other.Audience &&
        (TenantId?.Value ?? string.Empty) == (other.TenantId?.Value ?? string.Empty) &&
        (UserId?.Value ?? string.Empty) == (other.UserId?.Value ?? string.Empty) &&
        (ClientId?.Value ?? string.Empty) == (other.ClientId?.Value ?? string.Empty);
}
