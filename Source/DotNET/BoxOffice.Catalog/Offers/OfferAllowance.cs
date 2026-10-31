// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers;

/// <summary>
/// A compute allowance an offer includes.
/// </summary>
/// <param name="Meter">The meter.</param>
/// <param name="Quantity">How much of the meter per period.</param>
/// <param name="Period">How often the allowance renews.</param>
public record OfferAllowance(MeterId Meter, MeterQuantity Quantity, AllowancePeriod Period);
