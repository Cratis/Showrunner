// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Playbill.Seeing;

/// <summary>
/// How far a person has seen the release notes. It is the person's own: what one person has seen says nothing about another.
/// </summary>
/// <param name="Id">The person.</param>
/// <param name="LastSeen">The newest release the person has seen the notes of; <see cref="ReleaseVersion.None"/> when they have seen none.</param>
public record SeenReleaseNotes(ViewerId Id, ReleaseVersion LastSeen)
{
    /// <summary>
    /// Gets whether the person has seen the notes of a release: it is not newer than the newest they have seen.
    /// </summary>
    /// <param name="version">The release.</param>
    /// <returns><see langword="true"/> when they have; otherwise <see langword="false"/>.</returns>
    public bool HasSeen(ReleaseVersion version) => LastSeen.IsSet && version <= LastSeen;

    /// <summary>
    /// Moves how far the person has seen forward. It never moves back: seeing an older release again changes nothing.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <returns>The next state.</returns>
    public static SeenReleaseNotes Reduce(ReleaseNotesSeen @event, SeenReleaseNotes? current)
    {
        var id = ViewerId.For(@event.Viewer);
        if (current?.LastSeen.IsSet != true)
        {
            return new(id, @event.Version);
        }

        return @event.Version > current.LastSeen ? current with { LastSeen = @event.Version } : current;
    }
}
