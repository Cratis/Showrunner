// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Offering.Declaration;

/// <summary>
/// A meter of the product, as the product declares it.
/// </summary>
/// <param name="Id">The meter's key.</param>
/// <param name="Name">The meter's name as a person reads it.</param>
/// <param name="Unit">The unit the meter counts in, such as <c>minutes</c>.</param>
/// <param name="HasConcurrencyLimit">Whether the product also limits how much of the meter a tenant uses at the same time.</param>
public record MeterDefinition(
    MeterId Id,
    MeterName Name,
    MeterUnit Unit,
    bool HasConcurrencyLimit);
