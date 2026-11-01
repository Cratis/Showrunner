// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Compliance;
using Cratis.Chronicle.Compliance.GDPR;

namespace Cratis.Showrunner.Notes.Contracts.Giving;

/// <summary>
/// What the user wrote in a note. It can name or identify them.
/// </summary>
/// <param name="Value">The underlying value.</param>
[PII]
[ComplianceDetails("Free text the user writes to give feedback; it can name or identify them and is read by the people building the application.")]
public record NoteText(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="NoteText"/>.
    /// </summary>
    public static readonly NoteText NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="NoteText"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator NoteText(string value) => new(value);
}
