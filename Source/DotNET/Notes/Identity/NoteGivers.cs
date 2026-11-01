// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text;
using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;

namespace Cratis.Showrunner.Notes.Identity;

/// <summary>
/// The compliance subject of everything a user writes in a note: the person, not the note. Every fact the giver records
/// on any of their notes is encrypted under the same subject's key, so erasing the subject erases what they wrote on all
/// of their notes at once.
/// </summary>
/// <remarks>
/// The subject travels unencrypted with every event, also to the receiving side, which is not meant to learn from it who
/// gave a note. The subject is therefore a one-way hash of the user id, not the id itself. To erase a user, compute it
/// with <see cref="For"/> from their user id and delete its key in every tenant namespace they gave notes in.
/// </remarks>
public static class NoteGivers
{
    const string Prefix = "notes-giver-";

    /// <summary>
    /// Gets the compliance subject of a giver.
    /// </summary>
    /// <param name="giver">The user's id at their identity provider.</param>
    /// <returns>The <see cref="Subject"/>.</returns>
    public static Subject For(GiverUserId giver) =>
        new($"{Prefix}{Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(giver.Value)))}");

    /// <summary>
    /// Wraps a fact the giver records on a note so it is appended with the giver as its compliance subject.
    /// </summary>
    /// <param name="noteId">The note.</param>
    /// <param name="giver">The user's id at their identity provider.</param>
    /// <param name="fact">The fact.</param>
    /// <returns>The <see cref="EventForEventSourceId"/>.</returns>
    public static EventForEventSourceId Fact(NoteId noteId, GiverUserId giver, object fact) =>
        new(noteId, fact) { Subject = For(giver) };
}
