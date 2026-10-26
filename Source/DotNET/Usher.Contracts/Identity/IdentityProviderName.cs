// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Identity;

/// <summary>
/// The identity provider the reporter signed in to the application with, for example <c>aad</c> or <c>github</c>.
/// Together with <see cref="IdentityProviderSubject"/> it identifies the reporter.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record IdentityProviderName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="IdentityProviderName"/>.
    /// </summary>
    public static readonly IdentityProviderName NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to an <see cref="IdentityProviderName"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator IdentityProviderName(string value) => new(value);
}
