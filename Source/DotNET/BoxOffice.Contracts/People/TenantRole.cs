// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.People;

/// <summary>
/// The role a person holds in a tenant, as the product names it, such as <c>Maintainer</c>.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record TenantRole(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="TenantRole"/>.
    /// </summary>
    public static readonly TenantRole NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="TenantRole"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator TenantRole(string value) => new(value);
}
