// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Compliance;
using Cratis.Chronicle.Compliance.GDPR;

namespace Cratis.Showrunner.Marquee.People;

/// <summary>
/// Who a person is at the identity provider they signed in with: the subject it issued them.
/// </summary>
/// <param name="Value">The underlying value.</param>
[PII]
[ComplianceDetails("A pseudonymous identifier of the person at their identity provider; it records which announcements they have seen, so a dialog is shown once per person on every device.")]
public record UserSubject(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="UserSubject"/>.
    /// </summary>
    public static readonly UserSubject NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="UserSubject"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator UserSubject(string value) => new(value);
}
