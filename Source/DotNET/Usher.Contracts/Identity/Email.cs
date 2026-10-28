// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Compliance;
using Cratis.Chronicle.Compliance.GDPR;

namespace Cratis.Showrunner.Usher.Contracts.Identity;

/// <summary>
/// The email address of the reporter, as the identity provider gave it. It is a contact candidate, not evidence
/// that the mailbox was verified.
/// </summary>
/// <param name="Value">The underlying value.</param>
[PII]
[ComplianceDetails("Lets the support desk contact the reporter about their issue, only after the reporter chose to share who they are.")]
public record Email(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="Email"/>, used when the identity provider gave no email address.
    /// </summary>
    public static readonly Email NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to an <see cref="Email"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator Email(string value) => new(value);
}
