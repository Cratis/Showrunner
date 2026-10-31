// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers;

/// <summary>
/// An offer was made to include a compute allowance.
/// </summary>
/// <param name="Meter">The meter.</param>
/// <param name="Quantity">How much of the meter per period.</param>
/// <param name="Period">How often the allowance renews.</param>
[EventType]
public record OfferIncludedAllowance(MeterId Meter, MeterQuantity Quantity, AllowancePeriod Period);
