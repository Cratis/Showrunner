// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Seats;

/// <summary>
/// A person was given one of the tenant's seats. It is reported to the operator as <c>SeatTaken</c>.
/// </summary>
/// <param name="Holder">The person who holds the seat.</param>
[EventType]
public record SeatAssigned(Person Holder);

/// <summary>
/// A person no longer holds one of the tenant's seats. It is reported to the operator as <c>SeatReleased</c>.
/// </summary>
/// <param name="Holder">The person who held the seat.</param>
[EventType]
public record SeatUnassigned(Person Holder);
