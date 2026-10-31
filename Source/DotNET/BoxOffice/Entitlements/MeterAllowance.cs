// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Entitlements;

/// <summary>
/// What the operator last decided a tenant may use of a meter.
/// </summary>
/// <param name="Meter">The meter.</param>
/// <param name="Quantity">How much of the meter the tenant may use in a period.</param>
/// <param name="Period">The period the quantity is for.</param>
/// <param name="Overage">What happens beyond the quantity.</param>
/// <param name="DecidedAt">When the operator decided it.</param>
public record MeterAllowance(MeterId Meter, MeterQuantity Quantity, AllowancePeriod Period, OverageBehavior Overage, DateTimeOffset DecidedAt)
{
    /// <summary>
    /// Gets the allowance of a meter the operator set none for: nothing is allowed.
    /// </summary>
    /// <param name="meter">The meter.</param>
    /// <returns>The allowance of nothing.</returns>
    public static MeterAllowance None(MeterId meter) =>
        new(meter, MeterQuantity.NotSet, AllowancePeriod.Day, OverageBehavior.Block, DateTimeOffset.MinValue);
}
