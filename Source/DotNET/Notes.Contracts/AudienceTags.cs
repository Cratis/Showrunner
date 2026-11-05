// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts;

/// <summary>
/// The named tags that address Notes' channel between an application and the store that collects its notes.
/// </summary>
/// <remarks>
/// An application addresses a note to its collector with the Chronicle <c>audience</c> tag
/// (<c>NotesOptions.CollectorAudience</c>) and names itself with <see cref="From"/> (<c>NotesOptions.Audience</c>). The collector
/// keeps the requester on the private facts it records and addresses its replies to it, so the application's subscription
/// to that collector receives exactly the answers to its own notes. Nothing here changes an event schema.
/// </remarks>
public static class AudienceTags
{
    /// <summary>
    /// The tag name a note carries to identify the application that sent it; the collector addresses its replies to this value.
    /// </summary>
    public const string From = "from";
}
