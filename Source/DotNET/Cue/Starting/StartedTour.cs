// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Queries.ModelBound;

namespace Cratis.Showrunner.Cue.Starting;

/// <summary>
/// A guided tour the signed-in person has started.
/// </summary>
/// <param name="Id">The tour.</param>
[ReadModel]
public record StartedTour(TourId Id)
{
    /// <summary>
    /// Gets the tours the signed-in person has started.
    /// </summary>
    /// <remarks>
    /// Nothing is returned for someone who is not signed in. The answer follows the event log: a tour started a moment ago
    /// may not be in it yet, so a client keeps what it has just recorded and does not wait for it to come back.
    /// </remarks>
    /// <param name="tours">What each person has started.</param>
    /// <param name="viewer">The signed-in person.</param>
    /// <returns>The tours the person started.</returns>
    [Authorize]
    public static async Task<IEnumerable<StartedTour>> ByMe(IViewerTours tours, ISignedInViewer viewer)
    {
        if (!viewer.IsSignedIn)
        {
            return [];
        }

        var started = await tours.Of(viewer.Subject);
        return [.. started.Tours.Select(tour => new StartedTour(tour))];
    }
}
