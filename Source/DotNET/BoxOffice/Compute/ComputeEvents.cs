// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Compute;

/// <summary>
/// A unit of work was admitted against a meter's concurrency limit and counts as running until it stops or its lease ends.
/// Recorded only for a meter that has a concurrency limit, once per unit of work.
/// </summary>
/// <param name="Meter">The meter.</param>
/// <param name="Work">The unit of work.</param>
/// <param name="Until">When the unit of work stops counting as running when nobody ended it.</param>
[EventType]
public record ComputeWorkStarted(MeterId Meter, ComputeWorkId Work, DateTimeOffset Until);

/// <summary>
/// A unit of work that was admitted against a meter's concurrency limit ended and no longer holds its slot.
/// </summary>
/// <param name="Meter">The meter.</param>
/// <param name="Work">The unit of work.</param>
[EventType]
public record ComputeWorkStopped(MeterId Meter, ComputeWorkId Work);

/// <summary>
/// A maintainer of the product set how much of a meter the tenant may use in a period, below what the operator allows.
/// </summary>
/// <param name="Meter">The meter.</param>
/// <param name="Quantity">The most the tenant may use in the allowance's period.</param>
[EventType]
public record ComputeBudgetSet(MeterId Meter, MeterQuantity Quantity);

/// <summary>
/// A maintainer of the product set how many units of work of a meter the tenant may run at the same time.
/// </summary>
/// <param name="Meter">The meter.</param>
/// <param name="Limit">The number of units of work.</param>
[EventType]
public record ComputeConcurrencyLimitSet(MeterId Meter, int Limit);

/// <summary>
/// The tenant's allowance of a meter ran out in a period, so work was refused or parked. Recorded once per meter and period,
/// when the first unit of work is refused; the refusals after it are only logged and counted.
/// </summary>
/// <param name="Meter">The meter.</param>
/// <param name="Day">The day the allowance ran out.</param>
/// <param name="Period">The allowance's period.</param>
/// <param name="Overage">What happens to work that arrives once the allowance has run out.</param>
[EventType]
public record ComputeAllowanceReached(MeterId Meter, ComputeDay Day, AllowancePeriod Period, OverageBehavior Overage);
