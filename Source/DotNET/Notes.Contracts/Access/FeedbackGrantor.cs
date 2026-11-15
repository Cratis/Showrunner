// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Compliance;
using Cratis.Chronicle.Compliance.GDPR;

namespace Cratis.Showrunner.Notes.Contracts.Access;

/// <summary>
/// Who at the desk granted or revoked the right to give feedback, as the desk names its people: a pseudonymous id or a
/// name. It identifies a natural person, so it is personal data.
/// </summary>
/// <param name="Value">The underlying value.</param>
[PII]
[ComplianceDetails("Identifies the person at the desk who granted or revoked a user's right to give feedback, so the application can show who decided and the desk can be held to account.")]
public record FeedbackGrantor(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="FeedbackGrantor"/>.
    /// </summary>
    public static readonly FeedbackGrantor NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="FeedbackGrantor"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator FeedbackGrantor(string value) => new(value);
}
