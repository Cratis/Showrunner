// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Listing;

/// <summary>
/// Where a note is, as the giver sees it.
/// </summary>
public enum NoteStatus
{
    /// <summary>
    /// The giver gave it and nothing came back yet.
    /// </summary>
    Given = 0,

    /// <summary>
    /// The receiving side recorded it.
    /// </summary>
    Acknowledged = 1,

    /// <summary>
    /// A version that acts on it was rolled out.
    /// </summary>
    Addressed = 2,

    /// <summary>
    /// The receiving side decided not to act on it.
    /// </summary>
    Declined = 3,

    /// <summary>
    /// The giver withdrew it.
    /// </summary>
    Withdrawn = 4
}
