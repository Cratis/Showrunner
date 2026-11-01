// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts;

/// <summary>
/// Identifies the tenant a note belongs to: the name of the tenant's Chronicle namespace in the application. Every
/// Notes contract carries it in its payload, because the receiving side serves many applications and tenants and records
/// a note in the namespace this value names; it answers into the namespace this value names in the application.
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
