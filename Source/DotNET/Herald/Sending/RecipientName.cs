// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Compliance;
using Cratis.Chronicle.Compliance.GDPR;

namespace Cratis.Showrunner.Herald.Sending;

/// <summary>
/// The name shown beside the address an email is sent to.
/// </summary>
/// <param name="Value">The underlying value.</param>
[PII]
[ComplianceDetails("Shown beside the address so the recipient sees their name, until the email was sent or failed.")]
public record RecipientName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="RecipientName"/>.
    /// </summary>
    public static readonly RecipientName NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="RecipientName"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator RecipientName(string value) => new(value);
}
