// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Entitlements;

/// <summary>
/// How many seats a tenant may assign. A trial has seats until its last day, and seats granted or bought replace the
/// trial's, they do not add to them. Each part of the state follows the newest decision of the operator, whatever order
/// the decisions arrive in.
/// </summary>
/// <param name="Id">The id of the tenant's stream.</param>
/// <param name="Capacity">The seats the operator last decided.</param>
/// <param name="Until">The last day of the seats.</param>
/// <param name="Origin">Where the seats come from.</param>
/// <param name="TrialEnds">The last day of the trial, moved by the operator.</param>
/// <param name="TrialClosed">Whether the trial has closed.</param>
public record SeatCapacity(
    string Id,
    Stamped<SeatCount> Capacity,
    Stamped<EntitlementEnd> Until,
    Stamped<SeatCapacityOrigin> Origin,
    Stamped<TrialEnd> TrialEnds,
    Stamped<bool> TrialClosed)
{
    /// <summary>
    /// Gets the seats the tenant may assign on a day. A trial's seats end after its last day or when it closes, and the
    /// seats of a grant or purchase end after their last day, without any event.
    /// </summary>
    /// <param name="today">The day.</param>
    /// <returns>The number of seats.</returns>
    public int On(DateOnly today)
    {
        if (Origin.Value == SeatCapacityOrigin.Trial)
        {
            return TrialClosed.Value || today > TrialEnds.Value.Value ? 0 : Capacity.Value.Value;
        }

        return today > Until.Value.Value ? 0 : Capacity.Value.Value;
    }

    /// <summary>
    /// Gets whether the tenant's seats are the trial's and the trial is over, so the tenant has no seats it can use.
    /// </summary>
    /// <param name="today">The day.</param>
    /// <returns><see langword="true"/> when the trial is over and nothing granted or bought replaced its seats.</returns>
    public bool IsTrialOver(DateOnly today) =>
        Origin.Value == SeatCapacityOrigin.Trial && (TrialClosed.Value || today > TrialEnds.Value.Value);

    /// <summary>
    /// Folds <see cref="OperatorOpenedTrial"/> into the state.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event.</param>
    /// <returns>The next state.</returns>
    public static SeatCapacity Reduce(OperatorOpenedTrial @event, SeatCapacity? current, EventContext context) =>
        Of(current, context) with
        {
            Capacity = Of(current, context).Capacity.Decide(@event.Seats, context.Occurred),
            Until = Of(current, context).Until.Decide(new(@event.Ends.Value), context.Occurred),
            Origin = Of(current, context).Origin.Decide(SeatCapacityOrigin.Trial, context.Occurred),
            TrialEnds = Of(current, context).TrialEnds.Decide(@event.Ends, context.Occurred),
            TrialClosed = Of(current, context).TrialClosed.Decide(false, context.Occurred)
        };

    /// <summary>
    /// Folds <see cref="OperatorMovedTrialEnd"/> into the state.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event.</param>
    /// <returns>The next state.</returns>
    public static SeatCapacity Reduce(OperatorMovedTrialEnd @event, SeatCapacity? current, EventContext context) =>
        Of(current, context) with { TrialEnds = Of(current, context).TrialEnds.Decide(@event.Ends, context.Occurred) };

    /// <summary>
    /// Folds <see cref="OperatorClosedTrial"/> into the state.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event.</param>
    /// <returns>The next state.</returns>
    public static SeatCapacity Reduce(OperatorClosedTrial @event, SeatCapacity? current, EventContext context) =>
        Of(current, context) with { TrialClosed = Of(current, context).TrialClosed.Decide(true, context.Occurred) };

    /// <summary>
    /// Folds <see cref="OperatorSetSeatCapacity"/> into the state.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event.</param>
    /// <returns>The next state.</returns>
    public static SeatCapacity Reduce(OperatorSetSeatCapacity @event, SeatCapacity? current, EventContext context)
    {
        var standing = Of(current, context);
        var origin = @event.Source == SeatCapacitySource.Purchase ? SeatCapacityOrigin.Purchase : SeatCapacityOrigin.Grant;
        return standing with
        {
            Capacity = standing.Capacity.Decide(@event.Capacity, context.Occurred),
            Until = standing.Until.Decide(@event.Until, context.Occurred),
            Origin = standing.Origin.Decide(origin, context.Occurred)
        };
    }

    static SeatCapacity Of(SeatCapacity? current, EventContext context) => current ?? new(
        context.EventSourceId.Value,
        Stamped<SeatCount>.Undecided(SeatCount.NotSet),
        Stamped<EntitlementEnd>.Undecided(new(DateOnly.MinValue)),
        Stamped<SeatCapacityOrigin>.Undecided(SeatCapacityOrigin.Trial),
        Stamped<TrialEnd>.Undecided(new(DateOnly.MinValue)),
        Stamped<bool>.Undecided(false));
}
