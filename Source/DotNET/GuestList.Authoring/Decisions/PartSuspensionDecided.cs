// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Authoring.Decisions;

/// <summary>
/// Recorded when an operator decided to suspend a part that was not suspended at that level. It is the operator side's own
/// fact, in the stream of the product; the outbox publishes it as <c>PartSuspended</c>.
/// </summary>
/// <param name="Level">Whom the part is suspended for.</param>
/// <param name="TenantId">The tenant, for <see cref="SuspensionLevel.Tenant"/> and <see cref="SuspensionLevel.User"/>.</param>
/// <param name="UserId">The user, for <see cref="SuspensionLevel.User"/>.</param>
/// <param name="Part">The path of the part suspended.</param>
[EventType]
public record PartSuspensionDecided(SuspensionLevel Level, TenantId TenantId, UserId UserId, PartPath Part);
