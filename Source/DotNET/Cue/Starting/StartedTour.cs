// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Cratis.Arc.Queries.ModelBound;
using Cratis.Chronicle.ReadModels;
using ReactiveSubject = System.Reactive.Subjects.Subject;

namespace Cratis.Showrunner.Cue.Starting;

/// <summary>
/// A guided tour the signed-in person has started.
/// </summary>
/// <param name="Id">The tour.</param>
[ReadModel]
public record StartedTour(TourId Id)
{
    /// <summary>
    /// Observes the tours the signed-in person has started.
    /// </summary>
    /// <remarks>
    /// Nothing is returned for someone who is not signed in. The answer follows the event log: a tour started a moment ago
    /// may not be in it yet, so a client keeps what it has just recorded and does not wait for it to come back. It is observed
    /// rather than read once, so a tour the person starts on another device reaches this one without a reload.
    /// </remarks>
    /// <param name="readModels">The <see cref="IReadModels"/> of the current event store namespace.</param>
    /// <param name="viewer">The signed-in person.</param>
    /// <returns>The tours the person started, again every time they start another.</returns>
    [Authorize]
    public static ISubject<IEnumerable<StartedTour>> ByMe(IReadModels readModels, ISignedInViewer viewer)
    {
        if (!viewer.IsSignedIn)
        {
            return new BehaviorSubject<IEnumerable<StartedTour>>([]);
        }

        var id = ViewerId.For(viewer.Subject);
        var observed = readModels.Observe<StartedTours>(history => history.Id == id)
            .Select(histories => (IEnumerable<StartedTour>)[.. histories.SelectMany(history => history.Tours).Select(tour => new StartedTour(tour))]);
        return ReactiveSubject.Create<IEnumerable<StartedTour>>(Observer.Create<IEnumerable<StartedTour>>(_ => { }), observed);
    }
}
