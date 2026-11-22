// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Playbill.Seeing;

/// <summary>
/// Decides which releases a person has not been told about.
/// </summary>
public static class UnreadReleases
{
    /// <summary>
    /// Finds the releases newer than the newest a person has seen. A person who has seen none is told about the current
    /// release only, not the whole history: what is new for them is what the application is now, not everything that came
    /// before they arrived.
    /// </summary>
    /// <param name="releases">The releases, the newest first.</param>
    /// <param name="lastSeen">The newest release the person has seen the notes of; <see cref="ReleaseVersion.None"/> when they have seen none.</param>
    /// <returns>The releases they have not seen, the newest first.</returns>
    public static IReadOnlyList<ReleaseVersion> Since(IReadOnlyList<ReleaseVersion> releases, ReleaseVersion lastSeen) =>
        lastSeen.IsSet
            ? [.. releases.Where(release => release > lastSeen)]
            : [.. releases.Take(1)];

    /// <summary>
    /// Works out the status of a person who has seen the notes up to a version.
    /// </summary>
    /// <param name="notes">The release history.</param>
    /// <param name="lastSeen">The newest release the person has seen the notes of; <see cref="ReleaseVersion.None"/> when they have seen none.</param>
    /// <returns>The status.</returns>
    public static ReleaseNotesStatus StatusOf(IReleaseNotes notes, ReleaseVersion lastSeen)
    {
        var unread = Since([.. notes.All.Select(note => note.Version)], lastSeen);
        return new(notes.Current, lastSeen, unread.Count, unread);
    }
}
