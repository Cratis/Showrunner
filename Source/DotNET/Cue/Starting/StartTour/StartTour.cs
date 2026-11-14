// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.EventSequences;
using Cratis.Monads;

namespace Cratis.Showrunner.Cue.Starting.StartTour;

/// <summary>
/// Records that the signed-in person started a guided tour, so the application can stop offering it to them on every
/// device.
/// </summary>
/// <remarks>
/// Starting a tour a second time appends nothing: only the first start is a fact, and watching a tour again changes
/// nothing about who has seen it. A tour the application does not have is refused.
/// </remarks>
/// <param name="Tour">The tour.</param>
[Command]
[Authorize]
public record StartTour(TourId Tour)
{
    /// <summary>
    /// Finds out who is starting the tour and whether they already had.
    /// </summary>
    /// <param name="viewer">The signed-in person.</param>
    /// <param name="tours">What each person has started.</param>
    /// <returns>Who is starting the tour, or why they cannot.</returns>
    public async Task<Result<TourStart, ValidationResult>> Provide(ISignedInViewer viewer, IViewerTours tours)
    {
        if (!viewer.IsSignedIn)
        {
            return ValidationResult.Error("Sign in to start a tour.");
        }

        var started = await tours.Of(viewer.Subject);
        return new TourStart(viewer.Subject, started.Has(Tour));
    }

    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="start">Who is starting the tour and whether they already had.</param>
    /// <returns>The <see cref="TourStarted"/> event, or nothing when the person had already started the tour.</returns>
    public IEnumerable<EventForEventSourceId> Handle(TourStart start)
    {
        if (start.AlreadyStarted)
        {
            yield break;
        }

        yield return new EventForEventSourceId(ViewerId.For(start.Viewer), new TourStarted(Tour, start.Viewer))
        {
            Subject = ViewerId.SubjectOf(start.Viewer)
        };
    }
}
