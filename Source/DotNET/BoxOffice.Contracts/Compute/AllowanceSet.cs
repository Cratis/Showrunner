// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Compute;

/// <summary>
/// Published by Direct when a tenant's allowance for a meter is set or changed.
/// </summary>
/// <param name="TenantId">The tenant this concerns.</param>
/// <param name="Meter">The meter.</param>
/// <param name="Quantity">How much the tenant may use in each period.</param>
/// <param name="Period">How often the allowance renews.</param>
/// <param name="Overage">What happens when it is used up.</param>
[EventType]
[Public]
public record AllowanceSet(
    TenantId TenantId,
    MeterId Meter,
    MeterQuantity Quantity,
    AllowancePeriod Period,
    OverageBehavior Overage);
