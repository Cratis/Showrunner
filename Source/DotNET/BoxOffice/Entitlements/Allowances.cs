// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Entitlements;

/// <summary>
/// What a tenant may use of each meter.
/// </summary>
/// <param name="Id">The id of the tenant's stream.</param>
/// <param name="Meters">What the operator last decided about each meter.</param>
public record Allowances(string Id, IEnumerable<MeterAllowance> Meters)
{
    /// <summary>
    /// Gets the allowance of a meter.
    /// </summary>
    /// <param name="meter">The meter.</param>
    /// <returns>The allowance, nothing when the operator set none.</returns>
    public MeterAllowance For(MeterId meter) => Meters.FirstOrDefault(_ => _.Meter == meter) ?? MeterAllowance.None(meter);

    /// <summary>
    /// Folds <see cref="OperatorSetAllowance"/> into the state.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event.</param>
    /// <returns>The next state.</returns>
    public static Allowances Reduce(OperatorSetAllowance @event, Allowances? current, EventContext context)
    {
        var standing = current?.Meters ?? [];
        if (standing.FirstOrDefault(_ => _.Meter == @event.Meter) is { } earlier && earlier.DecidedAt > context.Occurred)
        {
            return current!;
        }

        return new(
            context.EventSourceId.Value,
            [.. standing.Where(_ => _.Meter != @event.Meter), new(@event.Meter, @event.Quantity, @event.Period, @event.Overage, context.Occurred)]);
    }
}
