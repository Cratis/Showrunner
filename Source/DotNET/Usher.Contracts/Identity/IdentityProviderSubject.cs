// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Identity;

/// <summary>
/// The stable subject the identity provider issued for the reporter. It is the key the application and the desk
/// correlate the reporter by, so it is not classified as personal data; the name and email are.
/// </summary>
/// <param name="Value">The underlying value.</param>
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
