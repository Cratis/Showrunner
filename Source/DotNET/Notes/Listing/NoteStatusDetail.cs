// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Listing;

/// <summary>
/// What the receiving side said about a note: the version that addresses it, or why it will not act on it.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record NoteStatusDetail(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="NoteStatusDetail"/>.
    /// </summary>
    public static readonly NoteStatusDetail NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="NoteStatusDetail"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator NoteStatusDetail(string value) => new(value);
}
