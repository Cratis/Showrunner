// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers;

/// <summary>
/// A compute pack offer was given the meter it extends and how much it adds.
/// </summary>
/// <param name="Meter">The meter.</param>
/// <param name="Quantity">How much the meter's allowance grows per period.</param>
/// <param name="Stackable">Whether an organization can hold more than one.</param>
[EventType]
public record ComputePackSpecified(MeterId Meter, MeterQuantity Quantity, bool Stackable);
