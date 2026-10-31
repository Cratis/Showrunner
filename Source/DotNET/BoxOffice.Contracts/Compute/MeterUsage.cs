// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Compute;

/// <summary>
/// The amount one meter counted.
/// </summary>
/// <param name="Meter">The meter.</param>
/// <param name="Quantity">The amount counted.</param>
public record MeterUsage(
    MeterId Meter,
    MeterQuantity Quantity);
