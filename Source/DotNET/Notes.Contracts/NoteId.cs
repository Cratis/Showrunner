// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts;

/// <summary>
/// Identifies one note. It is the event source id of every Notes event, private and public, in the application and on
/// the receiving side, and it never appears in an event payload. As an event source id it is the lowercase, hyphenated
/// <c>D</c> representation of the GUID.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record NoteId(Guid Value) : EventSourceId<Guid>(Value), IGeneratable<NoteId>
{
    /// <summary>
    /// Implicitly converts a <see cref="Guid"/> to a <see cref="NoteId"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator NoteId(Guid value) => new(value);

    /// <summary>
    /// Creates a new <see cref="NoteId"/>.
    /// </summary>
    /// <returns>A new <see cref="NoteId"/>.</returns>
    public static NoteId New() => new(GenerateValue.Uuid());
}
