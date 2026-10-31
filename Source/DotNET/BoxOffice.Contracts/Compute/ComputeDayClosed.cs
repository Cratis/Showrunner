// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Compute;

/// <summary>
/// Published by the product when a day's metering is final for a tenant.
/// </summary>
/// <param name="TenantId">The tenant this concerns.</param>
/// <param name="Day">The day.</param>
/// <param name="Usage">What each meter counted that day; empty when nothing was used.</param>
[EventType]
[Public]
public record ComputeDayClosed(TenantId TenantId, ComputeDay Day, IEnumerable<MeterUsage> Usage);
