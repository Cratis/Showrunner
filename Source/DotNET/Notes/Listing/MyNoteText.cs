// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Listing;

/// <summary>
/// What the giver wrote in a note, as their own list of notes shows it.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record MyNoteText(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="MyNoteText"/>.
    /// </summary>
    public static readonly MyNoteText NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="MyNoteText"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator MyNoteText(string value) => new(value);
}
