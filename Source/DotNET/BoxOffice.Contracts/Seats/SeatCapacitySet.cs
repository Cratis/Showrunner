// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Seats;

/// <summary>
/// Published by Direct when the number of seats a tenant may assign changes because seats were granted or
/// bought. The capacity is the total the tenant now has; it replaces the trial seats. Replaces Direct's
/// CustomerSeatsGranted and CustomerSeatsRevoked, and the v1 SeatsGrantedToTenant.
/// </summary>
/// <param name="TenantId">The tenant this concerns.</param>
/// <param name="Capacity">The total number of seats the tenant may assign.</param>
/// <param name="Until">The last day the capacity holds.</param>
/// <param name="Source">Where the capacity comes from.</param>
[EventType]
[Public]
public record SeatCapacitySet(TenantId TenantId, SeatCount Capacity, EntitlementEnd Until, SeatCapacitySource Source);
