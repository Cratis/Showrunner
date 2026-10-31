// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Catalog.Prices;

/// <summary>
/// The price arithmetic Direct's Admin catalog used, moved here and expressed in minor units: a yearly price is twelve
/// months less the yearly discount, and a quantity added in the middle of a period is charged for the days left of it.
/// Every amount is rounded to a whole minor unit, away from zero at the midpoint.
/// </summary>
public static class Pricing
{
    /// <summary>
    /// Gets what one period of an offer costs.
    /// </summary>
    /// <param name="monthlyPrice">The monthly price.</param>
    /// <param name="yearlyDiscount">The discount for paying yearly.</param>
    /// <param name="interval">The billing interval.</param>
    /// <returns>The total for the period.</returns>
    public static PriceInMinorUnits TotalFor(PriceInMinorUnits monthlyPrice, YearlyDiscount yearlyDiscount, BillingInterval interval) =>
        interval == BillingInterval.Yearly
            ? Round(monthlyPrice.Value * 12m * (1m - (yearlyDiscount.Value / 100m)))
            : monthlyPrice;

    /// <summary>
    /// Gets the day a period that starts on a day ends: the same day one interval later, which is the first day of the
    /// next period.
    /// </summary>
    /// <param name="start">The first day of the period.</param>
    /// <param name="interval">The billing interval.</param>
    /// <returns>The day the next period starts.</returns>
    public static DateOnly PeriodEnd(DateOnly start, BillingInterval interval) =>
        interval == BillingInterval.Yearly ? start.AddYears(1) : start.AddMonths(1);

    /// <summary>
    /// Gets what adding a quantity, such as seats, costs for the days left of the current period.
    /// </summary>
    /// <param name="recurringAmount">What the whole current quantity costs per period.</param>
    /// <param name="currentQuantity">How many are paid for.</param>
    /// <param name="addedQuantity">How many are added.</param>
    /// <param name="periodEnd">The end of the current period, as <see cref="PeriodEnd"/> gives it.</param>
    /// <param name="interval">The billing interval.</param>
    /// <param name="today">Today.</param>
    /// <returns>The prorated amount; nothing when no quantity is paid for yet, and nothing when the period is over.</returns>
    public static PriceInMinorUnits Prorate(
        PriceInMinorUnits recurringAmount,
        int currentQuantity,
        int addedQuantity,
        DateOnly periodEnd,
        BillingInterval interval,
        DateOnly today)
    {
        var periodStart = interval == BillingInterval.Yearly ? periodEnd.AddYears(-1) : periodEnd.AddMonths(-1);
        var periodDays = periodEnd.DayNumber - periodStart.DayNumber;
        var remainingDays = Math.Clamp(periodEnd.DayNumber - today.DayNumber, 0, periodDays);
        var perUnit = currentQuantity == 0 ? 0m : (decimal)recurringAmount.Value / currentQuantity;
        return Round(perUnit * addedQuantity * remainingDays / periodDays);
    }

    static PriceInMinorUnits Round(decimal amount) => (long)Math.Round(amount, 0, MidpointRounding.AwayFromZero);
}
