// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Compliance;
using Cratis.Chronicle.Compliance.GDPR;

namespace Cratis.Showrunner.Notes.Contracts.People;

/// <summary>
/// The display name of the user who gave a note, as the identity provider gave it; NotSet when it gave none.
/// </summary>
/// <param name="Value">The underlying value.</param>
[PII]
[ComplianceDetails("The display name of the user who gave a note, shown to the people reading the note so they can follow up with the giver.")]
public record GiverDisplayName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="GiverDisplayName"/>.
    /// </summary>
    public static readonly GiverDisplayName NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="GiverDisplayName"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator GiverDisplayName(string value) => new(value);
}
