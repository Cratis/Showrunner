// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.People;

/// <summary>
/// The subject a person is known by at their identity provider.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record IdentityProviderSubject(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="IdentityProviderSubject"/>.
    /// </summary>
    public static readonly IdentityProviderSubject NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="IdentityProviderSubject"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator IdentityProviderSubject(string value) => new(value);
}
