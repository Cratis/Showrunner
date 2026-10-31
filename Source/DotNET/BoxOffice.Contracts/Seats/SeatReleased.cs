// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Seats;

/// <summary>
/// Published by the product when a person gives up a seat. Replaces Direct's CustomerSeatReleased.
/// </summary>
/// <param name="TenantId">The tenant this concerns.</param>
/// <param name="Holder">The person who released the seat.</param>
[EventType]
[Public]
public record SeatReleased(TenantId TenantId, Person Holder);
