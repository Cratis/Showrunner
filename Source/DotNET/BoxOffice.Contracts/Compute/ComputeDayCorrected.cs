// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Compute;

/// <summary>
/// Published by the product when a day that was closed turns out to have counted wrongly. It replaces the usage
/// of <see cref="ComputeDayClosed"/> for that day.
/// </summary>
/// <param name="TenantId">The tenant this concerns.</param>
/// <param name="Day">The day.</param>
/// <param name="Usage">What each meter counted that day, as corrected.</param>
[EventType]
[Public]
public record ComputeDayCorrected(TenantId TenantId, ComputeDay Day, IEnumerable<MeterUsage> Usage);
