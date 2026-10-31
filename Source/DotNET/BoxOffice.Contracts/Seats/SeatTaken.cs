// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Seats;

/// <summary>
/// Published by the product when a person takes a seat. Replaces Direct's CustomerSeatAssigned.
/// </summary>
/// <param name="TenantId">The tenant this concerns.</param>
/// <param name="Holder">The person who took the seat.</param>
[EventType]
[Public]
public record SeatTaken(TenantId TenantId, Person Holder);
