// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.GuestList.Contracts.Audiences;

namespace Cratis.Showrunner.GuestList.Contracts.Availability;

/// <summary>
/// Published by the product, once per decision it recorded, to say that an operator decision reached it. It is the one
/// fact the operator cannot know for itself. A decision the product did not record, such as one for a tenant it does not
/// have, produces none, so the decision stays waiting. It is not a heartbeat and says nothing about the connection.
/// </summary>
/// <param name="Product">The product that recorded the decision.</param>
/// <param name="Environment">The environment the product reports from.</param>
/// <param name="Decision">Whether the audience was granted or withdrawn.</param>
/// <param name="Level">Whom the decision was for.</param>
/// <param name="TenantId">The tenant, for <see cref="AudienceLevel.Tenant"/> and <see cref="AudienceLevel.User"/>; empty otherwise.</param>
/// <param name="UserId">The user, for <see cref="AudienceLevel.User"/>; empty otherwise.</param>
/// <param name="Audience">The audience decided.</param>
/// <param name="DecidedAt">When the operator took the decision: its own occurred time, which pairs the receipt with it.</param>
[EventType]
[Public]
public record AudienceDecisionReceived(
    Product Product,
    EnvironmentName Environment,
    AudienceDecision Decision,
    AudienceLevel Level,
    TenantId TenantId,
    UserId UserId,
    AudienceName Audience,
    DateTimeOffset DecidedAt);
