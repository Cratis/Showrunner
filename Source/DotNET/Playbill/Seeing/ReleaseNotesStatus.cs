// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Cratis.Arc.Queries.ModelBound;
using Cratis.Chronicle.ReadModels;
using ReactiveSubject = System.Reactive.Subjects.Subject;

namespace Cratis.Showrunner.Playbill.Seeing;

/// <summary>
/// What the signed-in person has not been told about yet: the releases newer than the newest they have seen.
/// </summary>
/// <param name="Current">The version the application is running; empty when it is not known.</param>
/// <param name="LastSeen">The newest release the person has seen the notes of; empty when they have seen none.</param>
/// <param name="Unread">How many releases they have not seen the notes of.</param>
/// <param name="UnreadVersions">The versions of those releases, the newest first.</param>
[ReadModel]
public record ReleaseNotesStatus(ReleaseVersion Current, ReleaseVersion LastSeen, int Unread, IEnumerable<ReleaseVersion> UnreadVersions)
{
    /// <summary>
    /// Observes the signed-in person's status. It emits exactly one status, and a new one whenever they mark release notes as
    /// seen, so an unread badge clears on every device the person has open.
    /// </summary>
    /// <param name="readModels">The read models of the current event store namespace.</param>
    /// <param name="notes">The release history.</param>
    /// <param name="viewer">The signed-in person.</param>
    /// <returns>The observable status, empty when no one is signed in.</returns>
    [Authorize]
    public static ISubject<IEnumerable<ReleaseNotesStatus>> ForMe(IReadModels readModels, IReleaseNotes notes, ISignedInViewer viewer)
    {
        if (!viewer.IsSignedIn)
        {
            return new BehaviorSubject<IEnumerable<ReleaseNotesStatus>>([]);
        }

        var id = ViewerId.For(viewer.Subject);
        var observed = readModels.Observe<SeenReleaseNotes>(seen => seen.Id == id)
            .Select(seen => (IEnumerable<ReleaseNotesStatus>)[UnreadReleases.StatusOf(notes, seen.FirstOrDefault()?.LastSeen ?? ReleaseVersion.None)]);
        return ReactiveSubject.Create<IEnumerable<ReleaseNotesStatus>>(Observer.Create<IEnumerable<ReleaseNotesStatus>>(_ => { }), observed);
    }
}
