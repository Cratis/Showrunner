// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Tenants;

/// <summary>
/// The name of a tenant as a person reads it.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record TenantName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="TenantName"/>.
    /// </summary>
    public static readonly TenantName NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="TenantName"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator TenantName(string value) => new(value);
}
