// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Compliance;
using Cratis.Chronicle.Compliance.GDPR;

namespace Cratis.Showrunner.Notes.Contracts.People;

/// <summary>
/// The stable id of the user who gave a note: the subject the application's identity provider issued for the signed-in user. It is a pseudonymous identifier of a natural person, so it is personal data.
/// </summary>
/// <param name="Value">The underlying value.</param>
[PII]
[ComplianceDetails("A pseudonymous identifier of the user who gave a note at their identity provider; the application lists a user's notes by it and the receiving side learns it with the note.")]
public record GiverUserId(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="GiverUserId"/>.
    /// </summary>
    public static readonly GiverUserId NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="GiverUserId"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator GiverUserId(string value) => new(value);
}
