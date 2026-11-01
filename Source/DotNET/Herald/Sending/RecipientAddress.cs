// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Compliance;
using Cratis.Chronicle.Compliance.GDPR;

namespace Cratis.Showrunner.Herald.Sending;

/// <summary>
/// The address an email is sent to.
/// </summary>
/// <param name="Value">The underlying value.</param>
[PII]
[ComplianceDetails("Needed to deliver the email, until it was sent or failed.")]
public record RecipientAddress(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="RecipientAddress"/>.
    /// </summary>
    public static readonly RecipientAddress NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="RecipientAddress"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator RecipientAddress(string value) => new(value);
}
