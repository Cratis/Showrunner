// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.EventSequences;
using Cratis.Monads;

namespace Cratis.Showrunner.Playbill.Seeing.MarkReleaseNotesSeen;

/// <summary>
/// Records that the signed-in person has seen the release notes up to a version, so they are not told about those
/// releases again on any device.
/// </summary>
/// <remarks>
/// Marking a release the person has already seen appends nothing, and marking an older release than they have seen
/// never moves them back: only a step forward is a fact. A version that has no note is refused.
/// </remarks>
/// <param name="Version">The newest release the person has seen the notes of.</param>
[Command]
[Authorize]
public record MarkReleaseNotesSeen(ReleaseVersion Version)
{
    /// <summary>
    /// Finds out who has seen the notes and how far they had seen.
    /// </summary>
    /// <param name="viewer">The signed-in person.</param>
    /// <param name="notes">The release history.</param>
    /// <param name="seen">How far each person has seen the release notes.</param>
    /// <returns>Who has seen the notes and whether they had, or why they cannot have.</returns>
    public async Task<Result<SeenMark, ValidationResult>> Provide(ISignedInViewer viewer, IReleaseNotes notes, ISeenReleases seen)
    {
        if (!viewer.IsSignedIn)
        {
            return ValidationResult.Error("Sign in to mark release notes as seen.");
        }

        if (notes.Find(Version) is null)
        {
            return ValidationResult.Error("This release has no notes.");
        }

        var previous = await seen.Of(viewer.Subject);
        return new SeenMark(viewer.Subject, previous.HasSeen(Version));
    }

    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="mark">Who has seen the notes and whether they had.</param>
    /// <returns>The <see cref="ReleaseNotesSeen"/> event, or nothing when the person had already seen this far.</returns>
    public IEnumerable<EventForEventSourceId> Handle(SeenMark mark)
    {
        if (mark.AlreadySeen)
        {
            yield break;
        }

        yield return new EventForEventSourceId(ViewerId.For(mark.Viewer), new ReleaseNotesSeen(Version, mark.Viewer))
        {
            Subject = ViewerId.SubjectOf(mark.Viewer)
        };
    }
}
