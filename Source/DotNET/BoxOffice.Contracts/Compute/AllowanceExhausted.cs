// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Compute;

/// <summary>
/// Published by the product when a tenant uses up an allowance.
/// </summary>
/// <param name="TenantId">The tenant this concerns.</param>
/// <param name="Meter">The meter.</param>
/// <param name="Day">The day the allowance ran out.</param>
/// <param name="Overage">What the product does now.</param>
[EventType]
[Public]
public record AllowanceExhausted(TenantId TenantId, MeterId Meter, ComputeDay Day, OverageBehavior Overage);
