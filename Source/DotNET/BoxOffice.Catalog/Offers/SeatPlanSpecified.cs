// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers;

/// <summary>
/// A seat plan offer was given its seats.
/// </summary>
/// <param name="Seats">How many seats it carries; ignored when it is unlimited.</param>
/// <param name="Unlimited">Whether it carries unlimited seats.</param>
/// <param name="RequiresSalesContact">Whether it is bought through sales rather than checkout.</param>
[EventType]
public record SeatPlanSpecified(SeatCount Seats, bool Unlimited, bool RequiresSalesContact);
