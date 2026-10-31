// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Offering.Metering;

/// <summary>
/// What a meter measured from one of the product's own events: how much of the meter a tenant used, and on which day.
/// </summary>
/// <param name="Organization">The organization the tenant belongs to; the event source the day is reported under.</param>
/// <param name="Day">The UTC day the usage counts for, for example the day a session started.</param>
/// <param name="Quantity">How much was used, in the unit the meter is declared with.</param>
public record MeterReading(OrganizationId Organization, DateOnly Day, MeterQuantity Quantity);
