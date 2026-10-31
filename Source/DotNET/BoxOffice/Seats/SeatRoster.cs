// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Seats;

/// <summary>
/// Who holds the tenant's seats. Every assignment and release is recorded in the one stream, so a command that reads the
/// roster is guarded against any other assignment or release happening meanwhile.
/// </summary>
/// <param name="Id">The id of the tenant's stream.</param>
/// <param name="Holdings">The seats that are held, in the order they were assigned.</param>
public record SeatRoster(string Id, IEnumerable<SeatHolding> Holdings)
{
    /// <summary>
    /// Gets the seats held.
    /// </summary>
    public int Held => Holdings.Count();

    /// <summary>
    /// Gets whether a person holds a seat, whether or not the tenant's capacity still covers it.
    /// </summary>
    /// <param name="holder">Who.</param>
    /// <returns><see langword="true"/> when they hold a seat.</returns>
    public bool Holds(SeatHolder holder) => Holdings.Any(_ => SeatHolder.Of(_.Holder) == holder);

    /// <summary>
    /// Gets the holders the tenant's capacity licenses. A pool that shrinks below what is held licenses the holders who
    /// were assigned first, and leaves the others unlicensed.
    /// </summary>
    /// <param name="capacity">The seats the tenant may use.</param>
    /// <returns>The licensed holders, oldest first.</returns>
    public IEnumerable<SeatHolding> Licensed(int capacity) => Holdings.OrderBy(_ => _.AssignedAt).Take(Math.Max(capacity, 0));

    /// <summary>
    /// Gets whether the capacity licenses a person's seat.
    /// </summary>
    /// <param name="holder">Who.</param>
    /// <param name="capacity">The seats the tenant may use.</param>
    /// <returns><see langword="true"/> when they hold a seat the capacity covers.</returns>
    public bool IsLicensed(SeatHolder holder, int capacity) => Licensed(capacity).Any(_ => SeatHolder.Of(_.Holder) == holder);

    /// <summary>
    /// Folds <see cref="SeatAssigned"/> into the state.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event.</param>
    /// <returns>The next state.</returns>
    public static SeatRoster Reduce(SeatAssigned @event, SeatRoster? current, EventContext context)
    {
        var standing = current ?? new(context.EventSourceId.Value, []);
        return standing.Holds(SeatHolder.Of(@event.Holder))
            ? standing
            : standing with { Holdings = [.. standing.Holdings, new(@event.Holder, context.Occurred)] };
    }

    /// <summary>
    /// Folds <see cref="SeatUnassigned"/> into the state.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event.</param>
    /// <returns>The next state.</returns>
    public static SeatRoster? Reduce(SeatUnassigned @event, SeatRoster? current, EventContext context)
    {
        var holder = SeatHolder.Of(@event.Holder);
        return current is null ? null : current with { Holdings = [.. current.Holdings.Where(_ => SeatHolder.Of(_.Holder) != holder)] };
    }
}
