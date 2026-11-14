// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.GuestList.Contracts.Audiences;

namespace Cratis.Showrunner.GuestList.Contracts.Availability;

/// <summary>
/// Generation 1 of <see cref="AudienceDecisionReceived"/>, as it shipped: no system is named.
/// </summary>
/// <param name="Product">The product that recorded the decision.</param>
/// <param name="Environment">The environment the product reports from.</param>
/// <param name="Decision">Whether the audience was granted or withdrawn.</param>
/// <param name="Level">Whom the decision was for.</param>
/// <param name="TenantId">The tenant, for the tenant and user levels; empty otherwise.</param>
/// <param name="UserId">The user, for the user level; empty otherwise.</param>
/// <param name="Audience">The audience decided.</param>
/// <param name="DecidedAt">When the operator took the decision.</param>
[EventTypeGenerationFor<AudienceDecisionReceived>(1)]
public record AudienceDecisionReceivedV1(
    Product Product,
    EnvironmentName Environment,
    AudienceDecision Decision,
    AudienceLevelV1 Level,
    TenantId TenantId,
    UserId UserId,
    AudienceName Audience,
    DateTimeOffset DecidedAt);
