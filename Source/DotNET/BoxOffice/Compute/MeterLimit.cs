// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Compute;

/// <summary>
/// What the product's maintainers limited of a meter for the tenant.
/// </summary>
/// <param name="Meter">The meter.</param>
/// <param name="Budget">The most the tenant may use in the allowance's period, when a budget was set.</param>
/// <param name="ConcurrencyLimit">How many units of work the tenant may run at the same time, when a limit was set.</param>
public record MeterLimit(MeterId Meter, MeterQuantity? Budget, int? ConcurrencyLimit);
