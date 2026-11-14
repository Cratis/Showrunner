// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Compliance;
using Cratis.Chronicle.Compliance.GDPR;

namespace Cratis.Showrunner.Cue.Viewers;

/// <summary>
/// Who a person is at the identity provider they signed in with: the subject it issued them.
/// </summary>
/// <param name="Value">The underlying value.</param>
[PII]
[ComplianceDetails("A pseudonymous identifier of the person at their identity provider; it records which guided tours they have started, so a tour is offered once per person on every device.")]
public record ViewerSubject(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="ViewerSubject"/>.
    /// </summary>
    public static readonly ViewerSubject NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="ViewerSubject"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator ViewerSubject(string value) => new(value);
}
