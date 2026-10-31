// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Seats;

/// <summary>
/// A seat a person holds.
/// </summary>
/// <param name="Holder">The person who holds the seat.</param>
/// <param name="AssignedAt">When the seat was assigned.</param>
public record SeatHolding(Person Holder, DateTimeOffset AssignedAt);
