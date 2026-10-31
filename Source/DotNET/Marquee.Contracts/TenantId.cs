// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Marquee.Contracts;

/// <summary>
/// Identifies a tenant of a product: the name of the tenant's Chronicle namespace in the product. It is Marquee's own
/// concept, so Marquee does not depend on any other Showrunner part. Announcements belong to a product, not to a tenant,
/// so it appears only inside an <see cref="Audiences.Audience"/> that names tenants.
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
