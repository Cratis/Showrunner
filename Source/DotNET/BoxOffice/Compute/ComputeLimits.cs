// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Queries.ModelBound;
using Cratis.Chronicle.ReadModels;

namespace Cratis.Showrunner.BoxOffice.Compute;

/// <summary>
/// The budgets and concurrency limits the product's maintainers set for the tenant, per meter.
/// </summary>
/// <param name="Id">The id of the tenant's stream.</param>
/// <param name="Meters">What was limited of each meter.</param>
[ReadModel]
public record ComputeLimits(string Id, IEnumerable<MeterLimit> Meters)
{
    /// <summary>
    /// Gets what was limited of a meter.
    /// </summary>
    /// <param name="meter">The meter.</param>
    /// <returns>The limits, with nothing limited when the maintainers set none.</returns>
    public MeterLimit For(MeterId meter) => Meters.FirstOrDefault(_ => _.Meter == meter) ?? new(meter, null, null);

    /// <summary>
    /// Gets the budgets and concurrency limits of the tenant, for the product's maintainers.
    /// </summary>
    /// <param name="readModels">The <see cref="IReadModels"/> of the current tenant.</param>
    /// <returns>The limits, with nothing limited when the maintainers set none.</returns>
    [Authorize(Policy = BoxOfficePolicies.ManageCompute)]
    public static async Task<ComputeLimits> CurrentLimits(IReadModels readModels) =>
        await readModels.GetInstanceById<ComputeLimits>(TenantStream.Id.Value) ?? new(TenantStream.Id.Value, []);

    /// <summary>
    /// Folds <see cref="ComputeBudgetSet"/> into the state.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event.</param>
    /// <returns>The next state.</returns>
    public static ComputeLimits Reduce(ComputeBudgetSet @event, ComputeLimits? current, EventContext context)
    {
        var standing = current ?? new(context.EventSourceId.Value, []);
        return standing.With(standing.For(@event.Meter) with { Budget = @event.Quantity });
    }

    /// <summary>
    /// Folds <see cref="ComputeConcurrencyLimitSet"/> into the state.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event.</param>
    /// <returns>The next state.</returns>
    public static ComputeLimits Reduce(ComputeConcurrencyLimitSet @event, ComputeLimits? current, EventContext context)
    {
        var standing = current ?? new(context.EventSourceId.Value, []);
        return standing.With(standing.For(@event.Meter) with { ConcurrencyLimit = @event.Limit });
    }

    ComputeLimits With(MeterLimit limit) => this with { Meters = [.. Meters.Where(_ => _.Meter != limit.Meter), limit] };
}
