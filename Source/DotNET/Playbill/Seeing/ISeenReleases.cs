// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Playbill.Seeing;

/// <summary>
/// Defines how far each person has seen the release notes.
/// </summary>
public interface ISeenReleases
{
    /// <summary>
    /// Gets how far a person has seen the release notes.
    /// </summary>
    /// <param name="viewer">The person.</param>
    /// <returns>What they have seen; nothing when they have seen none.</returns>
    Task<SeenReleaseNotes> Of(ViewerSubject viewer);
}
