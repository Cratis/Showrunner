// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Offering;

/// <summary>
/// A meter a product declares.
/// </summary>
/// <param name="Id">The meter's id.</param>
/// <param name="Name">The meter's name.</param>
/// <param name="Unit">The unit the meter counts in.</param>
public record MeterDeclaration(
    MeterId Id,
    MeterName Name,
    MeterUnit Unit);
