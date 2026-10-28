// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts;

/// <summary>
/// Identifies the tenant an issue belongs to: the name of the tenant's Chronicle namespace in the application.
/// Every Usher contract carries it in its payload, because a desk receives issues from many tenants and the
/// application routes a desk event back into the namespace this value names.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record TenantId(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="TenantId"/>.
    /// </summary>
    public static readonly TenantId NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="TenantId"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator TenantId(string value) => new(value);
}
