// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Cue.Starting;

/// <summary>
/// The guided tours a person has started. It is the person's own: what one person started says nothing about another.
/// </summary>
/// <param name="Id">The person.</param>
/// <param name="Tours">The tours they started, in the order they first started them.</param>
public record StartedTours(ViewerId Id, IEnumerable<TourId> Tours)
{
    /// <summary>
    /// Gets whether the person started a tour.
    /// </summary>
    /// <param name="tour">The tour.</param>
    /// <returns><see langword="true"/> when they did; otherwise <see langword="false"/>.</returns>
    public bool Has(TourId tour) => Tours?.Contains(tour) == true;

    /// <summary>
    /// Records that a person started a tour, unless they already had.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The next state.</returns>
    public static StartedTours Reduce(TourStarted @event, StartedTours? current)
    {
        var id = ViewerId.For(@event.Viewer);
        if (current is null)
        {
            return new(id, [@event.Tour]);
        }

        return current.Has(@event.Tour) ? current : current with { Tours = [.. current.Tours, @event.Tour] };
    }
}
