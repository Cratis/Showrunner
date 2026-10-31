// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice.Offering.Metering;

namespace Cratis.Showrunner.BoxOffice.Compute;

/// <summary>
/// Reads how much of a meter a tenant has used in an allowance period, from the days the product measured.
/// </summary>
public static class MeterConsumption
{
    /// <summary>
    /// Gets the first day of the allowance period a day falls in.
    /// </summary>
    /// <param name="period">The allowance period.</param>
    /// <param name="today">The day.</param>
    /// <returns>The first day of the period.</returns>
    public static DateOnly PeriodStart(AllowancePeriod period, DateOnly today) =>
        period == AllowancePeriod.Month ? new DateOnly(today.Year, today.Month, 1) : today;

    /// <summary>
    /// Gets the first day of the period after the one a day falls in.
    /// </summary>
    /// <param name="period">The allowance period.</param>
    /// <param name="today">The day.</param>
    /// <returns>The first day of the next period.</returns>
    public static DateOnly NextPeriodStart(AllowancePeriod period, DateOnly today) =>
        period == AllowancePeriod.Month ? new DateOnly(today.Year, today.Month, 1).AddMonths(1) : today.AddDays(1);

    /// <summary>
    /// Gets how much of a meter the tenant has used since the allowance period began.
    /// </summary>
    /// <param name="metering">The store of measured days.</param>
    /// <param name="tenant">The tenant.</param>
    /// <param name="meter">The meter.</param>
    /// <param name="period">The allowance period.</param>
    /// <param name="today">The day the period is read on.</param>
    /// <returns>The quantity used.</returns>
    public static async Task<long> UsedInPeriod(IMeteringStore metering, TenantId tenant, MeterId meter, AllowancePeriod period, DateOnly today)
    {
        var from = PeriodStart(period, today);
        var days = Enumerable.Range(0, today.DayNumber - from.DayNumber + 1).Select(offset => new ComputeDay(from.AddDays(offset)));
        var buckets = await Task.WhenAll(days.Select(day => metering.Get(tenant, day)));
        return buckets.Sum(_ => _ is not null && _.Quantities.TryGetValue(meter, out var quantity) ? quantity.Value : 0L);
    }
}
