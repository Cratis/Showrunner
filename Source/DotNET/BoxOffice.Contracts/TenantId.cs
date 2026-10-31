// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts;

/// <summary>
/// Identifies a tenant in the product: the name of the tenant's Chronicle namespace. Every tenant-scoped Box
/// Office event carries it in its payload, because Direct serves many tenants and a product routes an event
/// from Direct into the namespace this value names. In Direct it is the lobby's tenant name.
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
