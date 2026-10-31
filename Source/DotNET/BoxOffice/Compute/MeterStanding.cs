// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Queries.ModelBound;
using Cratis.Chronicle.ReadModels;
using Cratis.Showrunner.BoxOffice.Entitlements;
using Cratis.Showrunner.BoxOffice.Offering.Declaration;
using Cratis.Showrunner.BoxOffice.Offering.Metering;

namespace Cratis.Showrunner.BoxOffice.Compute;

/// <summary>
/// Where the tenant stands with one meter in the allowance period it is in now: what it has used, and what it may use.
/// </summary>
/// <param name="Meter">The meter.</param>
/// <param name="Name">What the meter is called.</param>
/// <param name="Unit">What the meter counts.</param>
/// <param name="Used">How much the tenant has used since the period began.</param>
/// <param name="Allowance">How much the operator allows in the period; zero when the operator set no allowance.</param>
/// <param name="Period">The period the allowance and what is used count over.</param>
/// <param name="Overage">What happens to work that asks for more than the allowance.</param>
/// <param name="PeriodEnds">The first day of the next period, when the used quantity starts from zero again.</param>
/// <param name="Budget">The most the tenant's own administrators decided to use in the period, when they set one.</param>
/// <param name="ConcurrencyLimit">How many units of work may run at the same time, when a limit was set.</param>
/// <param name="HasConcurrencyLimit">Whether the product limits how many units of this meter run at the same time.</param>
[ReadModel]
public record MeterStanding(
    MeterId Meter,
    MeterName Name,
    MeterUnit Unit,
    long Used,
    long Allowance,
    AllowancePeriod Period,
    OverageBehavior Overage,
    DateOnly PeriodEnds,
    long? Budget,
    int? ConcurrencyLimit,
    bool HasConcurrencyLimit)
{
    /// <summary>
    /// Gets the standing of every meter the product declared, in the order of their names.
    /// </summary>
    /// <param name="offering">What the product declared.</param>
    /// <param name="entitlements">The tenant's entitlements.</param>
    /// <param name="readModels">The read models.</param>
    /// <param name="metering">The store of measured days.</param>
    /// <param name="eventStore">The event store of the tenant, whose namespace is the tenant.</param>
    /// <param name="time">The clock.</param>
    /// <returns>The standing of every meter.</returns>
    [Authorize(Policy = BoxOfficePolicies.ManageCompute)]
    public static async Task<IEnumerable<MeterStanding>> CurrentUsage(
        ProductOffering offering,
        IEntitlements entitlements,
        IReadModels readModels,
        IMeteringStore metering,
        Cratis.Chronicle.IEventStore eventStore,
        TimeProvider time)
    {
        var today = DateOnly.FromDateTime(time.GetUtcNow().UtcDateTime);
        var tenant = new TenantId(eventStore.Namespace.Value);
        var limits = await readModels.GetInstanceById<ComputeLimits>(TenantStream.Id.Value) ?? new(TenantStream.Id.Value, []);

        var standings = new List<MeterStanding>();
        foreach (var definition in offering.Meters.OrderBy(_ => _.Name.Value, StringComparer.Ordinal))
        {
            var allowance = await entitlements.AllowanceFor(definition.Id);
            var limit = limits.For(definition.Id);
            standings.Add(new(
                definition.Id,
                definition.Name,
                definition.Unit,
                await MeterConsumption.UsedInPeriod(metering, tenant, definition.Id, allowance.Period, today),
                allowance.Quantity.Value,
                allowance.Period,
                allowance.Overage,
                MeterConsumption.NextPeriodStart(allowance.Period, today),
                limit.Budget?.Value,
                limit.ConcurrencyLimit,
                definition.HasConcurrencyLimit));
        }

        return standings;
    }
}
