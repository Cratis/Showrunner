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
public record HeldAudience(AudienceLevel Level, TenantId TenantId, UserId UserId, AudienceName Audience);
