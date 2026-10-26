// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Compliance;
using Cratis.Chronicle.Compliance.GDPR;

namespace Cratis.Showrunner.Usher.Contracts.Identity;

/// <summary>
/// The stable subject the identity provider issued for the reporter. Together with the identity provider it identifies
/// the reporter, so the application finds their issues and the desk correlates them by it. It is a pseudonymous
/// identifier of a natural person, so it is personal data: erasing the reporter makes it unreadable along with
/// everything else they wrote.
/// </summary>
/// <param name="Value">The underlying value.</param>
[PII]
[ComplianceDetails("A pseudonymous identifier of the reporter at their identity provider; the application matches the reporter's issues to them by it, and the desk learns it only when the reporter shares who they are.")]
public record IdentityProviderSubject(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="IdentityProviderSubject"/>.
    /// </summary>
    public static readonly IdentityProviderSubject NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to an <see cref="IdentityProviderSubject"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator IdentityProviderSubject(string value) => new(value);
}
